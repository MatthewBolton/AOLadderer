using AOLadderer.ClusterTemplates;
using AOLadderer.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AOLadderer.Blazor.Models
{
    public class ImplantModel
    {
        private readonly Dictionary<string, ShinyClusterTemplate> shinyClusterHash;
        private readonly Dictionary<string, BrightClusterTemplate> brightClusterHash;
        private readonly Dictionary<string, FadedClusterTemplate> fadedClusterHash;

        public ImplantModel(ImplantSlot slot)
        {
            this.Slot = slot;
            shinyClusterHash = slot.ShinyClusterTemplates.ToDictionary(t => t.Stat.Name);
            brightClusterHash = slot.BrightClusterTemplates.ToDictionary(t => t.Stat.Name);
            fadedClusterHash = slot.FadedClusterTemplates.ToDictionary(t => t.Stat.Name);
        }

        public ImplantSlot Slot { get; }
        public string ShinyClusterSelection { get; set; } = string.Empty; // Empty string is more compact than null when serialized.
        public string BrightClusterSelection { get; set; } = string.Empty; // Empty string is more compact than null when serialized.
        public string FadedClusterSelection { get; set; } = string.Empty; // Empty string is more compact than null when serialized.
        public bool IsUnavailable { get; set; }

        public IReadOnlyCollection<string> ShinyClusterOptions => shinyClusterHash.Keys;
        public IReadOnlyCollection<string> BrightClusterOptions => brightClusterHash.Keys;
        public IReadOnlyCollection<string> FadedClusterOptions => fadedClusterHash.Keys;

        public string GetBestShinyClusterSelection(IReadOnlyDictionary<string, double> targetWeights, int implantQl)
            => GetBestClusterSelection(shinyClusterHash, targetWeights, implantQl);

        public string GetBestBrightClusterSelection(IReadOnlyDictionary<string, double> targetWeights, int implantQl)
            => GetBestClusterSelection(brightClusterHash, targetWeights, implantQl);

        public string GetBestFadedClusterSelection(IReadOnlyDictionary<string, double> targetWeights, int implantQl)
            => GetBestClusterSelection(fadedClusterHash, targetWeights, implantQl);
        
        public ImplantTemplate CreateImplantTemplate()
        {
            shinyClusterHash.TryGetValue(ShinyClusterSelection ?? string.Empty, out ShinyClusterTemplate shinyCluster);
            brightClusterHash.TryGetValue(BrightClusterSelection ?? string.Empty, out BrightClusterTemplate brightCluster);
            fadedClusterHash.TryGetValue(FadedClusterSelection ?? string.Empty, out FadedClusterTemplate fadedCluster);
            if (shinyCluster == null && brightCluster == null && fadedCluster == null) return null;

            return ImplantTemplate.GetImplantTemplate(Slot, shinyCluster?.Stat, brightCluster?.Stat, fadedCluster?.Stat);
        }

        public string GetLabel()
        {
            ImplantTemplate template = CreateImplantTemplate();
            if (template == null) return Slot.ShortName;

            return $"{template.RequiredAbility.ShortName} {Slot.ShortName}";
        }

        public int GetDirectContribution(string targetName, int implantQl)
        {
            if (implantQl <= 0 || string.IsNullOrWhiteSpace(targetName)) return 0;

            int total = 0;

            if (shinyClusterHash.TryGetValue(ShinyClusterSelection ?? string.Empty, out ShinyClusterTemplate shinyCluster)
                && targetName.Equals(shinyCluster.Stat.Name, StringComparison.Ordinal))
            {
                total += shinyCluster.GetStatIncrease(implantQl);
            }

            if (brightClusterHash.TryGetValue(BrightClusterSelection ?? string.Empty, out BrightClusterTemplate brightCluster)
                && targetName.Equals(brightCluster.Stat.Name, StringComparison.Ordinal))
            {
                total += brightCluster.GetStatIncrease(implantQl);
            }

            if (fadedClusterHash.TryGetValue(FadedClusterSelection ?? string.Empty, out FadedClusterTemplate fadedCluster)
                && targetName.Equals(fadedCluster.Stat.Name, StringComparison.Ordinal))
            {
                total += fadedCluster.GetStatIncrease(implantQl);
            }

            return total;
        }

        public double GetTrickledownContribution(string targetName, int implantQl)
        {
            if (implantQl <= 0 || string.IsNullOrWhiteSpace(targetName)) return 0;
            if (!Stat.TryGetStat(targetName, out Stat targetStat) || !(targetStat is Skill targetSkill)) return 0;

            return GetSelectedClusters()
                .Where(cluster => cluster.RaisesAbility)
                .Sum(cluster => targetSkill.GetTrickledownGain(cluster.Ability, cluster.GetStatIncrease(implantQl)));
        }

        public double GetTotalContribution(string targetName, int implantQl)
            => GetDirectContribution(targetName, implantQl) + GetTrickledownContribution(targetName, implantQl);

        private IEnumerable<ClusterTemplate> GetSelectedClusters()
        {
            if (shinyClusterHash.TryGetValue(ShinyClusterSelection ?? string.Empty, out ShinyClusterTemplate shinyCluster))
            {
                yield return shinyCluster;
            }

            if (brightClusterHash.TryGetValue(BrightClusterSelection ?? string.Empty, out BrightClusterTemplate brightCluster))
            {
                yield return brightCluster;
            }

            if (fadedClusterHash.TryGetValue(FadedClusterSelection ?? string.Empty, out FadedClusterTemplate fadedCluster))
            {
                yield return fadedCluster;
            }
        }

        private static string GetBestClusterSelection<TClusterTemplate>(
            IReadOnlyDictionary<string, TClusterTemplate> clusterHash,
            IReadOnlyDictionary<string, double> targetWeights,
            int implantQl)
            where TClusterTemplate : ClusterTemplate
        {
            if (targetWeights.Count == 0) return string.Empty;

            return clusterHash
                .Select(kvp => new
                {
                    Selection = kvp.Key,
                    Score = targetWeights.Sum(target => target.Value * GetClusterContribution(kvp.Value, target.Key, implantQl))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Selection)
                .Select(x => x.Selection)
                .FirstOrDefault() ?? string.Empty;
        }

        private static double GetClusterContribution(ClusterTemplate clusterTemplate, string targetName, int implantQl)
        {
            int statIncrease = clusterTemplate.GetStatIncrease(implantQl);
            double directContribution = targetName.Equals(clusterTemplate.Stat.Name, StringComparison.Ordinal)
                ? statIncrease
                : 0;

            if (!(clusterTemplate.Stat is Ability ability)
                || !Stat.TryGetStat(targetName, out Stat targetStat)
                || !(targetStat is Skill targetSkill))
            {
                return directContribution;
            }

            return directContribution + targetSkill.GetTrickledownGain(ability, statIncrease);
        }
    }

    public class ImplantsModel : IReadOnlyCollection<ImplantModel>, IUrlTokenSerializable
    {
        private const int AutofillReferenceQl = 200;
        private static readonly double[] AutofillPriorityWeights = new[] { 3d, 2d, 1d };
        private readonly IReadOnlyCollection<ImplantModel> implants
            = ImplantSlot.ImplantSlots
            .Select(s => new ImplantModel(s))
            .ToArray();
        private sealed class PresetAppliedSelection
        {
            public bool Shiny { get; set; }
            public bool Bright { get; set; }
            public bool Faded { get; set; }
        }

        private readonly Dictionary<ImplantSlot, PresetAppliedSelection> presetAppliedSelections
            = new Dictionary<ImplantSlot, PresetAppliedSelection>();

        public int Count => implants.Count;
        public bool HasAppliedPresetClusters => presetAppliedSelections.Count > 0;
        public IReadOnlyCollection<string> AutofillTargetOptions => implants
            .SelectMany(i => i.ShinyClusterOptions.Concat(i.BrightClusterOptions).Concat(i.FadedClusterOptions))
            .Distinct()
            .OrderBy(name => name)
            .ToArray();

        public IEnumerator<ImplantModel> GetEnumerator() => implants.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => implants.GetEnumerator();

        public void ApplyTargetContributionAutofill(IEnumerable<string> targets)
        {
            IReadOnlyDictionary<string, double> targetWeights = targets
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .Take(AutofillPriorityWeights.Length)
                .Select((target, index) => new { Target = target, Weight = AutofillPriorityWeights[index] })
                .ToDictionary(x => x.Target, x => x.Weight, StringComparer.Ordinal);

            foreach (ImplantModel implant in implants.Where(i => !i.IsUnavailable))
            {
                implant.ShinyClusterSelection = implant.GetBestShinyClusterSelection(targetWeights, AutofillReferenceQl);
                implant.BrightClusterSelection = implant.GetBestBrightClusterSelection(targetWeights, AutofillReferenceQl);
                implant.FadedClusterSelection = implant.GetBestFadedClusterSelection(targetWeights, AutofillReferenceQl);
            }
        }

        public void ResetClusterSelections()
        {
            foreach (ImplantModel implant in implants)
            {
                implant.ShinyClusterSelection = string.Empty;
                implant.BrightClusterSelection = string.Empty;
                implant.FadedClusterSelection = string.Empty;
            }

            presetAppliedSelections.Clear();
        }

        public bool ApplyPreset(AoNormalizedPreset preset)
        {
            if (preset == null || preset.SlotSelections.Count == 0)
            {
                presetAppliedSelections.Clear();
                return false;
            }

            presetAppliedSelections.Clear();
            Dictionary<ImplantSlot, ImplantModel> implantsBySlot = implants.ToDictionary(i => i.Slot);

            foreach (AoPresetSlotSelection slotSelection in preset.SlotSelections)
            {
                if (!implantsBySlot.TryGetValue(slotSelection.Slot, out ImplantModel implant))
                {
                    continue;
                }

                if (implant.IsUnavailable)
                {
                    continue;
                }

                PresetAppliedSelection appliedSelection = new PresetAppliedSelection();

                if (slotSelection.HasShiny)
                {
                    implant.ShinyClusterSelection = slotSelection.ShinyCluster;
                    appliedSelection.Shiny = true;
                }

                if (slotSelection.HasBright)
                {
                    implant.BrightClusterSelection = slotSelection.BrightCluster;
                    appliedSelection.Bright = true;
                }

                if (slotSelection.HasFaded)
                {
                    implant.FadedClusterSelection = slotSelection.FadedCluster;
                    appliedSelection.Faded = true;
                }

                if (appliedSelection.Shiny || appliedSelection.Bright || appliedSelection.Faded)
                {
                    presetAppliedSelections[slotSelection.Slot] = appliedSelection;
                }
            }

            return presetAppliedSelections.Count > 0;
        }

        public void ClearAppliedPresetClusters()
        {
            foreach (ImplantModel implant in implants)
            {
                if (!presetAppliedSelections.TryGetValue(implant.Slot, out PresetAppliedSelection appliedSelection))
                {
                    continue;
                }

                if (appliedSelection.Shiny)
                {
                    implant.ShinyClusterSelection = string.Empty;
                }

                if (appliedSelection.Bright)
                {
                    implant.BrightClusterSelection = string.Empty;
                }

                if (appliedSelection.Faded)
                {
                    implant.FadedClusterSelection = string.Empty;
                }
            }

            presetAppliedSelections.Clear();
        }

        public void UrlTokenDeserialize(Queue<object> data)
        {
            foreach (ImplantModel implant in implants)
            {
                implant.ShinyClusterSelection = Convert.ToString(data.Dequeue());
                implant.BrightClusterSelection = Convert.ToString(data.Dequeue());
                implant.FadedClusterSelection = Convert.ToString(data.Dequeue());
                implant.IsUnavailable = Convert.ToString(data.Dequeue()) == "1";
            }
        }

        public void UrlTokenSerialize(Queue<object> data)
        {
            foreach (ImplantModel implant in implants)
            {
                data.Enqueue(implant.ShinyClusterSelection);
                data.Enqueue(implant.BrightClusterSelection);
                data.Enqueue(implant.FadedClusterSelection);
                data.Enqueue(implant.IsUnavailable ? 1 : 0);
            }
        }
    }
}
