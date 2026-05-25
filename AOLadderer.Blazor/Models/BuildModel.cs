using System;
using AOLadderer.LadderProcesses;
using AOLadderer.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AOLadderer.Blazor.Models
{
    public class BuildModel : IUrlTokenSerializable
    {
        private const int OptimizerMaxPasses = 3;
        private const int OptimizerBeamWidth = 4;
        private const int OptimizerBeamDepth = 2;
        private const double ObjectiveImprovementEpsilon = 0.000001d;
        private static readonly IReadOnlyDictionary<int, double> TargetPriorityWeights = new Dictionary<int, double>
        {
            [1] = 9d,
            [2] = 3d,
            [3] = 1d
        };

        private sealed class OptimizationState
        {
            public OptimizationState(string[] selections, double objective, int distanceFromBaseline)
            {
                Selections = selections;
                Objective = objective;
                DistanceFromBaseline = distanceFromBaseline;
            }

            public string[] Selections { get; }
            public double Objective { get; }
            public int DistanceFromBaseline { get; }
        }

        public class TargetStatModel
        {
            public int Priority { get; init; }
            public string Target { get; init; } = string.Empty;
            public double CurrentValue { get; init; }
            public double GoalValue { get; init; }
            public bool HasTarget => !string.IsNullOrWhiteSpace(Target);
            public bool HasGoal => GoalValue > 0;
        }

        public class TargetProjectionModel
        {
            public int Priority { get; init; }
            public string Target { get; init; } = string.Empty;
            public double CurrentValue { get; init; }
            public double GoalValue { get; init; }
            public double DirectGain { get; init; }
            public double TrickledownGain { get; init; }
            public double TotalImplantDrivenGain => DirectGain + TrickledownGain;
            public double ProjectedTotal => CurrentValue + TotalImplantDrivenGain;
            public double DeficitToGoal => Math.Max(0, GoalValue - ProjectedTotal);
        }

        public BuildModel()
        {
            if (PresetProfessionOptions.Count > 0)
            {
                PresetProfession = PresetProfessionOptions.First();
            }

            EnsurePresetVariantSelection();
        }

        public ImplantsModel Implants { get; } = new ImplantsModel();
        public StatsModel Stats { get; } = new StatsModel();
        public BuffsModel Buffs { get; } = new BuffsModel();
        public LadderMode LadderMode { get; set; } = LadderMode.Basic;
        public bool ShopBuyableQLMode { get; set; }
        public string PresetProfession { get; set; } = string.Empty;
        public string PresetId { get; set; } = string.Empty;
        public string AutofillTarget1 { get; set; } = string.Empty;
        public string AutofillTarget2 { get; set; } = string.Empty;
        public string AutofillTarget3 { get; set; } = string.Empty;
        public double AutofillTarget1Current { get; set; }
        public double AutofillTarget2Current { get; set; }
        public double AutofillTarget3Current { get; set; }
        public double AutofillTarget1Goal { get; set; }
        public double AutofillTarget2Goal { get; set; }
        public double AutofillTarget3Goal { get; set; }

        public IReadOnlyCollection<string> PresetProfessionOptions => AoUniversePresetCatalog.ProfessionOptions;
        public IReadOnlyList<AoNormalizedPreset> PresetOptionsForSelectedProfession
            => AoUniversePresetCatalog.GetPresetsForProfession(PresetProfession);
        public bool HasSelectedPreset
            => !string.IsNullOrWhiteSpace(PresetId)
                && AoUniversePresetCatalog.TryGetPresetById(PresetId, out _);
        public bool HasAppliedPresetClusters => Implants.HasAppliedPresetClusters;
        public IReadOnlyCollection<string> AutofillTargetOptions => Implants.AutofillTargetOptions;
        public IReadOnlyList<string> AutofillTargets => new[] { AutofillTarget1, AutofillTarget2, AutofillTarget3 };
        public bool HasAutofillTargets => AutofillTargets.Any(t => !string.IsNullOrWhiteSpace(t));
        public bool HasOptimizationGoals => TargetStats.Any(t => t.HasTarget && t.HasGoal);
        public IReadOnlyList<TargetStatModel> TargetStats => new[]
        {
            new TargetStatModel { Priority = 1, Target = AutofillTarget1, CurrentValue = AutofillTarget1Current, GoalValue = AutofillTarget1Goal },
            new TargetStatModel { Priority = 2, Target = AutofillTarget2, CurrentValue = AutofillTarget2Current, GoalValue = AutofillTarget2Goal },
            new TargetStatModel { Priority = 3, Target = AutofillTarget3, CurrentValue = AutofillTarget3Current, GoalValue = AutofillTarget3Goal }
        };
        // Populated by AppState after it builds the ladder; cleared on any Build change.
        // Null means the ladder hasn't been run yet — fall back to base-stat QL.
        private IReadOnlyDictionary<ImplantSlot, int> _ladderFinalQLBySlot;

        public void SetLadderFinalQLCache(IEnumerable<Implant> finalImplants)
            => _ladderFinalQLBySlot = finalImplants.ToDictionary(i => i.ImplantSlot, i => i.QL);

        public void InvalidateLadderFinalQLCache()
            => _ladderFinalQLBySlot = null;

        public IReadOnlyList<TargetProjectionModel> TargetProjections
        {
            get
            {
                var activeTargets = TargetStats.Where(t => t.HasTarget).ToList();
                if (activeTargets.Count == 0) return System.Array.Empty<TargetProjectionModel>();

                return activeTargets.Select(t =>
                {
                    (double directGain, double trickledownGain) = _ladderFinalQLBySlot != null
                        ? GetImplantContribution(t.Target, _ladderFinalQLBySlot)
                        : GetBaseStatImplantContribution(t.Target);
                    return new TargetProjectionModel
                    {
                        Priority = t.Priority,
                        Target = t.Target,
                        CurrentValue = t.CurrentValue,
                        GoalValue = t.GoalValue,
                        DirectGain = directGain,
                        TrickledownGain = trickledownGain
                    };
                }).ToArray();
            }
        }

        public LadderProcess CreateLadderProcess()
        {
            var character = new Character(
                Stats.Agility, Stats.Intelligence, Stats.Psychic,
                Stats.Sense, Stats.Stamina, Stats.Strength, Stats.Treatment)
            {
                ShopBuyableQLMode = ShopBuyableQLMode
            };
            var implantTemplates = Implants
                .Where(i => !i.IsUnavailable)
                .Select(i => i.CreateImplantTemplate())
                .Where(i => i != null)
                .ToArray();
            var unavailableImplantSlots = Implants
                .Where(i => i.IsUnavailable)
                .Select(i => i.Slot)
                .ToArray();

            return LadderMode switch
            {
                LadderMode.Advanced => new AdvancedLadderProcess(character, implantTemplates, unavailableImplantSlots),
                _ => new BasicLadderProcess(character, implantTemplates, unavailableImplantSlots)
            };
        }

        public void ApplyAutofill()
            => Implants.ApplyTargetContributionAutofill(AutofillTargets);

        public void SetPresetProfession(string profession)
        {
            PresetProfession = profession ?? string.Empty;
            EnsurePresetVariantSelection();
        }

        public void ApplySelectedPreset()
        {
            if (!AoUniversePresetCatalog.TryGetPresetById(PresetId, out AoNormalizedPreset preset))
            {
                return;
            }

            Implants.ApplyPreset(preset);
        }

        public void ClearAppliedPresetClusters()
            => Implants.ClearAppliedPresetClusters();

        public void OptimizeForGoals() => OptimizeForGoalsCore(yieldBetweenSeeds: false).GetAwaiter().GetResult();

        public Task OptimizeForGoalsAsync() => OptimizeForGoalsCore(yieldBetweenSeeds: true);

        private async Task OptimizeForGoalsCore(bool yieldBetweenSeeds)
        {
            TargetStatModel[] optimizationTargets = TargetStats
                .Where(t => t.HasTarget && t.HasGoal)
                .ToArray();
            if (optimizationTargets.Length == 0) return;

            string[] originalSelections = CaptureSelectionState();
            double baselineObjective = EvaluateWeightedGoalDeficit(optimizationTargets);
            OptimizationState bestState = new OptimizationState(originalSelections, baselineObjective, 0);

            try
            {
                foreach (string[] seedSelections in BuildOptimizationSeeds(originalSelections, optimizationTargets))
                {
                    OptimizationState candidate = SearchSeed(seedSelections, optimizationTargets, originalSelections);

                    if (IsStrictlyBetterState(candidate, bestState))
                    {
                        bestState = candidate;
                    }

                    // Yield to the browser between seeds so the JS event loop stays alive
                    // and the "Page Unresponsive" dialog doesn't fire on long optimizations.
                    if (yieldBetweenSeeds)
                        await Task.Yield();
                }
            }
            finally
            {
                ApplySelectionState(originalSelections);
            }

            if (bestState.Objective < baselineObjective - ObjectiveImprovementEpsilon)
            {
                ApplySelectionState(bestState.Selections);
            }
        }

        public void ResetAutofillClusters()
            => Implants.ResetClusterSelections();

        private void EnsurePresetVariantSelection()
        {
            IReadOnlyList<AoNormalizedPreset> presets = PresetOptionsForSelectedProfession;

            if (presets.Count == 0)
            {
                PresetId = string.Empty;
                return;
            }

            bool selectedPresetIsValid = presets.Any(p => p.Id.Equals(PresetId, StringComparison.Ordinal));

            if (!selectedPresetIsValid)
            {
                PresetId = presets[0].Id;
            }
        }

        private bool OptimizeClusterSelection(
            ImplantModel implant,
            IReadOnlyCollection<TargetStatModel> optimizationTargets,
            IEnumerable<string> options,
            Func<string> getSelection,
            Action<string> setSelection)
        {
            string originalSelection = getSelection() ?? string.Empty;
            string bestSelection = originalSelection;
            double bestObjective = EvaluateWeightedGoalDeficit(optimizationTargets);

            foreach (string option in EnumerateSelectionOptions(originalSelection, options))
            {
                setSelection(option);
                double objective = EvaluateWeightedGoalDeficit(optimizationTargets);

                bool hasLowerObjective = objective < bestObjective - ObjectiveImprovementEpsilon;

                if (hasLowerObjective)
                {
                    bestSelection = option;
                    bestObjective = objective;
                }
            }

            setSelection(bestSelection);
            return !string.Equals(originalSelection, bestSelection, StringComparison.Ordinal);
        }

        private IEnumerable<string[]> BuildOptimizationSeeds(
            string[] originalSelections,
            IReadOnlyCollection<TargetStatModel> optimizationTargets)
        {
            var seenKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (string[] seedSelections in CreateOrderedSeeds(originalSelections, optimizationTargets))
            {
                string key = GetSelectionStateKey(seedSelections);

                if (seenKeys.Add(key))
                {
                    yield return seedSelections;
                }
            }
        }

        private IEnumerable<string[]> CreateOrderedSeeds(
            string[] originalSelections,
            IReadOnlyCollection<TargetStatModel> optimizationTargets)
        {
            yield return CloneSelectionState(originalSelections);

            ApplySelectionState(originalSelections);
            RunStrictLocalSearch(optimizationTargets);
            yield return CaptureSelectionState();

            string[] orderedTargets = optimizationTargets
                .OrderBy(t => t.Priority)
                .Select(t => t.Target)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int offset = 0; offset < orderedTargets.Length; offset++)
            {
                string[] rotatedTargets = orderedTargets
                    .Skip(offset)
                    .Concat(orderedTargets.Take(offset))
                    .ToArray();

                ApplySelectionState(originalSelections);
                Implants.ApplyTargetContributionAutofill(rotatedTargets);
                yield return CaptureSelectionState();

                RunStrictLocalSearch(optimizationTargets);
                yield return CaptureSelectionState();
            }

            ApplySelectionState(originalSelections);
        }

        private OptimizationState SearchSeed(
            string[] seedSelections,
            IReadOnlyCollection<TargetStatModel> optimizationTargets,
            string[] baselineSelections)
        {
            OptimizationState bestState = CreateOptimizationState(seedSelections, optimizationTargets, baselineSelections);
            List<OptimizationState> frontier = new List<OptimizationState> { bestState };
            var bestByKey = new Dictionary<string, OptimizationState>(StringComparer.Ordinal)
            {
                [GetSelectionStateKey(bestState.Selections)] = bestState
            };

            for (int depth = 0; depth < OptimizerBeamDepth; depth++)
            {
                var nextFrontier = new List<OptimizationState>();

                foreach (OptimizationState state in frontier)
                {
                    foreach (string[] neighborSelections in EnumerateNeighborStates(state.Selections))
                    {
                        OptimizationState neighbor = CreateOptimizationState(neighborSelections, optimizationTargets, baselineSelections);
                        string neighborKey = GetSelectionStateKey(neighbor.Selections);

                        if (bestByKey.TryGetValue(neighborKey, out OptimizationState existing)
                            && !IsStrictlyBetterState(neighbor, existing))
                        {
                            continue;
                        }

                        bestByKey[neighborKey] = neighbor;
                        nextFrontier.Add(neighbor);

                        if (IsStrictlyBetterState(neighbor, bestState))
                        {
                            bestState = neighbor;
                        }
                    }
                }

                frontier = nextFrontier
                    .OrderBy(state => state.Objective)
                    .ThenBy(state => state.DistanceFromBaseline)
                    .Take(OptimizerBeamWidth)
                    .ToList();

                if (frontier.Count == 0)
                {
                    break;
                }
            }

            ApplySelectionState(bestState.Selections);
            RunStrictLocalSearch(optimizationTargets);
            return CreateOptimizationState(CaptureSelectionState(), optimizationTargets, baselineSelections);
        }

        private bool RunStrictLocalSearch(IReadOnlyCollection<TargetStatModel> optimizationTargets)
        {
            bool hasAnyImprovements = false;

            for (int pass = 0; pass < OptimizerMaxPasses; pass++)
            {
                bool hasPassImprovements = false;

                foreach (ImplantModel implant in Implants.Where(i => !i.IsUnavailable))
                {
                    hasPassImprovements |= OptimizeClusterSelection(
                        implant,
                        optimizationTargets,
                        implant.ShinyClusterOptions,
                        getSelection: () => implant.ShinyClusterSelection,
                        setSelection: selection => implant.ShinyClusterSelection = selection);

                    hasPassImprovements |= OptimizeClusterSelection(
                        implant,
                        optimizationTargets,
                        implant.BrightClusterOptions,
                        getSelection: () => implant.BrightClusterSelection,
                        setSelection: selection => implant.BrightClusterSelection = selection);

                    hasPassImprovements |= OptimizeClusterSelection(
                        implant,
                        optimizationTargets,
                        implant.FadedClusterOptions,
                        getSelection: () => implant.FadedClusterSelection,
                        setSelection: selection => implant.FadedClusterSelection = selection);
                }

                hasAnyImprovements |= hasPassImprovements;

                if (!hasPassImprovements)
                {
                    break;
                }
            }

            return hasAnyImprovements;
        }

        private IEnumerable<string[]> EnumerateNeighborStates(string[] selections)
        {
            ImplantModel[] implants = Implants.ToArray();

            for (int implantIndex = 0; implantIndex < implants.Length; implantIndex++)
            {
                ImplantModel implant = implants[implantIndex];
                if (implant.IsUnavailable)
                {
                    continue;
                }

                int selectionIndex = implantIndex * 3;

                foreach (string[] neighbor in EnumerateNeighborStatesForSelection(selections, selectionIndex, implant.ShinyClusterOptions))
                {
                    yield return neighbor;
                }

                foreach (string[] neighbor in EnumerateNeighborStatesForSelection(selections, selectionIndex + 1, implant.BrightClusterOptions))
                {
                    yield return neighbor;
                }

                foreach (string[] neighbor in EnumerateNeighborStatesForSelection(selections, selectionIndex + 2, implant.FadedClusterOptions))
                {
                    yield return neighbor;
                }
            }
        }

        private IEnumerable<string[]> EnumerateNeighborStatesForSelection(
            string[] selections,
            int selectionIndex,
            IEnumerable<string> options)
        {
            string currentSelection = selections[selectionIndex] ?? string.Empty;

            foreach (string option in EnumerateSelectionOptions(currentSelection, options))
            {
                if (string.Equals(option, currentSelection, StringComparison.Ordinal))
                {
                    continue;
                }

                string[] nextSelections = CloneSelectionState(selections);
                nextSelections[selectionIndex] = option;
                yield return nextSelections;
            }
        }

        private static IEnumerable<string> EnumerateSelectionOptions(string currentSelection, IEnumerable<string> options)
        {
            yield return currentSelection ?? string.Empty;

            foreach (string option in options
                .Where(option => !string.IsNullOrEmpty(option))
                .Distinct(StringComparer.Ordinal)
                .Where(option => !string.Equals(option, currentSelection, StringComparison.Ordinal)))
            {
                yield return option;
            }

            if (!string.IsNullOrEmpty(currentSelection))
            {
                yield return string.Empty;
            }
        }

        private OptimizationState CreateOptimizationState(
            string[] selections,
            IReadOnlyCollection<TargetStatModel> optimizationTargets,
            string[] baselineSelections)
        {
            string[] capturedSelections = CloneSelectionState(selections);
            ApplySelectionState(capturedSelections);

            return new OptimizationState(
                capturedSelections,
                EvaluateWeightedGoalDeficit(optimizationTargets),
                CountSelectionDifferences(capturedSelections, baselineSelections));
        }

        private string[] CaptureSelectionState()
            => Implants
                .SelectMany(implant => new[]
                {
                    implant.ShinyClusterSelection ?? string.Empty,
                    implant.BrightClusterSelection ?? string.Empty,
                    implant.FadedClusterSelection ?? string.Empty
                })
                .ToArray();

        private void ApplySelectionState(IReadOnlyList<string> selections)
        {
            ImplantModel[] implants = Implants.ToArray();

            for (int implantIndex = 0; implantIndex < implants.Length; implantIndex++)
            {
                int selectionIndex = implantIndex * 3;
                implants[implantIndex].ShinyClusterSelection = selections[selectionIndex] ?? string.Empty;
                implants[implantIndex].BrightClusterSelection = selections[selectionIndex + 1] ?? string.Empty;
                implants[implantIndex].FadedClusterSelection = selections[selectionIndex + 2] ?? string.Empty;
            }
        }

        private static string[] CloneSelectionState(IReadOnlyList<string> selections)
            => selections.Select(selection => selection ?? string.Empty).ToArray();

        private static int CountSelectionDifferences(IReadOnlyList<string> left, IReadOnlyList<string> right)
        {
            int differences = 0;

            for (int index = 0; index < left.Count; index++)
            {
                if (!string.Equals(left[index], right[index], StringComparison.Ordinal))
                {
                    differences++;
                }
            }

            return differences;
        }

        private static string GetSelectionStateKey(IReadOnlyList<string> selections)
            => string.Join("\n", selections.Select(selection => selection ?? string.Empty));

        private static bool IsStrictlyBetterState(OptimizationState candidate, OptimizationState incumbent)
        {
            if (candidate.Objective < incumbent.Objective - ObjectiveImprovementEpsilon)
            {
                return true;
            }

            return Math.Abs(candidate.Objective - incumbent.Objective) <= ObjectiveImprovementEpsilon
                && candidate.DistanceFromBaseline < incumbent.DistanceFromBaseline;
        }

        private double EvaluateWeightedGoalDeficit(IEnumerable<TargetStatModel> optimizationTargets)
            => optimizationTargets.Sum(target =>
            {
                (double directGain, double trickledownGain) = GetBaseStatImplantContribution(target.Target);
                double projectedValue = target.CurrentValue + directGain + trickledownGain;
                double deficit = Math.Max(0, target.GoalValue - projectedValue);
                double priorityWeight = TargetPriorityWeights.TryGetValue(target.Priority, out double weight)
                    ? weight
                    : 1d;

                return priorityWeight * deficit;
            });

        // Used by the optimizer, which scores many candidate configs — no ladder run per call.
        private (double DirectGain, double TrickledownGain) GetBaseStatImplantContribution(string target)
        {
            if (string.IsNullOrWhiteSpace(target)) return (0, 0);

            double directGain = 0;
            double trickledownGain = 0;

            foreach (var implantSelection in Implants
                .Where(i => !i.IsUnavailable)
                .Select(i => new { Implant = i, Template = i.CreateImplantTemplate() })
                .Where(x => x.Template != null))
            {
                int implantQl = Implant.GetMaxImplantQL(
                    GetAbilityValue(implantSelection.Template.RequiredAbility),
                    Stats.Treatment);
                if (ShopBuyableQLMode)
                    implantQl = Implant.ClampToShopBuyableQL(implantQl);
                directGain += implantSelection.Implant.GetDirectContribution(target, implantQl);
                trickledownGain += implantSelection.Implant.GetTrickledownContribution(target, implantQl);
            }

            return (directGain, trickledownGain);
        }

        private (double DirectGain, double TrickledownGain) GetImplantContribution(
            string target, IReadOnlyDictionary<ImplantSlot, int> finalQLBySlot)
        {
            if (string.IsNullOrWhiteSpace(target)) return (0, 0);

            double directGain = 0;
            double trickledownGain = 0;

            foreach (var implantSelection in Implants
                .Where(i => !i.IsUnavailable)
                .Select(i => new { Implant = i, Template = i.CreateImplantTemplate() })
                .Where(x => x.Template != null))
            {
                int implantQl;
                if (!finalQLBySlot.TryGetValue(implantSelection.Template.ImplantSlot, out implantQl))
                {
                    implantQl = Implant.GetMaxImplantQL(
                        GetAbilityValue(implantSelection.Template.RequiredAbility),
                        Stats.Treatment);
                    if (ShopBuyableQLMode)
                        implantQl = Implant.ClampToShopBuyableQL(implantQl);
                }
                directGain += implantSelection.Implant.GetDirectContribution(target, implantQl);
                trickledownGain += implantSelection.Implant.GetTrickledownContribution(target, implantQl);
            }

            return (directGain, trickledownGain);
        }

        private int GetAbilityValue(Ability ability)
            => ability?.Name switch
            {
                nameof(Ability.Strength) => Stats.Strength,
                nameof(Ability.Agility) => Stats.Agility,
                nameof(Ability.Stamina) => Stats.Stamina,
                nameof(Ability.Intelligence) => Stats.Intelligence,
                nameof(Ability.Sense) => Stats.Sense,
                nameof(Ability.Psychic) => Stats.Psychic,
                _ => 0
            };

        public void UrlTokenDeserialize(Queue<object> data)
        {
            Implants.UrlTokenDeserialize(data);
            Stats.UrlTokenDeserialize(data);
            Buffs.UrlTokenDeserialize(data);
            LadderMode = data.Count > 0
                && int.TryParse(Convert.ToString(data.Dequeue()), out int ladderModeValue)
                && System.Enum.IsDefined(typeof(LadderMode), ladderModeValue)
                    ? (LadderMode)ladderModeValue
                    : LadderMode.Basic;
            AutofillTarget1 = data.Count > 0 ? Convert.ToString(data.Dequeue()) ?? string.Empty : string.Empty;
            AutofillTarget2 = data.Count > 0 ? Convert.ToString(data.Dequeue()) ?? string.Empty : string.Empty;
            AutofillTarget3 = data.Count > 0 ? Convert.ToString(data.Dequeue()) ?? string.Empty : string.Empty;
            AutofillTarget1Current = TryReadDouble(data);
            AutofillTarget2Current = TryReadDouble(data);
            AutofillTarget3Current = TryReadDouble(data);
            AutofillTarget1Goal = TryReadDouble(data);
            AutofillTarget2Goal = TryReadDouble(data);
            AutofillTarget3Goal = TryReadDouble(data);
            PresetProfession = data.Count > 0 ? Convert.ToString(data.Dequeue()) ?? string.Empty : string.Empty;
            PresetId = data.Count > 0 ? Convert.ToString(data.Dequeue()) ?? string.Empty : string.Empty;
            ShopBuyableQLMode = data.Count > 0
                && int.TryParse(Convert.ToString(data.Dequeue()), out int shopModeValue)
                && shopModeValue != 0;
            if (string.IsNullOrWhiteSpace(PresetProfession) && PresetProfessionOptions.Count > 0)
            {
                PresetProfession = PresetProfessionOptions.First();
            }
            EnsurePresetVariantSelection();

            Stats.Apply(Buffs);
        }

        private static double TryReadDouble(Queue<object> data)
        {
            if (data.Count == 0) return 0;

            return double.TryParse(Convert.ToString(data.Dequeue()), out double value)
                ? value
                : 0;
        }

        public void UrlTokenSerialize(Queue<object> data)
        {
            Implants.UrlTokenSerialize(data);
            Stats.UrlTokenSerialize(data);
            Buffs.UrlTokenSerialize(data);
            data.Enqueue((int)LadderMode);
            data.Enqueue(AutofillTarget1);
            data.Enqueue(AutofillTarget2);
            data.Enqueue(AutofillTarget3);
            data.Enqueue(AutofillTarget1Current);
            data.Enqueue(AutofillTarget2Current);
            data.Enqueue(AutofillTarget3Current);
            data.Enqueue(AutofillTarget1Goal);
            data.Enqueue(AutofillTarget2Goal);
            data.Enqueue(AutofillTarget3Goal);
            data.Enqueue(PresetProfession);
            data.Enqueue(PresetId);
            data.Enqueue(ShopBuyableQLMode ? 1 : 0);
        }
    }
}
