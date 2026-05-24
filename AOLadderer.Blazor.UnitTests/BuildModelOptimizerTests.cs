using System;
using System.Linq;
using AOLadderer.Blazor.Models;
using Xunit;

namespace AOLadderer.Blazor.UnitTests
{
    public class BuildModelOptimizerTests
    {
        [Fact]
        public void OptimizeForGoals_DoesNotIncreaseWeightedDeficit()
        {
            BuildModel model = CreateHighStatsBuild();
            model.ResetAutofillClusters();

            string primary = model.AutofillTargetOptions.Contains("Treatment")
                ? "Treatment"
                : model.AutofillTargetOptions.First();
            string secondary = model.AutofillTargetOptions.Contains("Agility")
                ? "Agility"
                : model.AutofillTargetOptions.Skip(1).FirstOrDefault() ?? primary;

            model.AutofillTarget1 = primary;
            model.AutofillTarget1Current = 0;
            model.AutofillTarget1Goal = 300;
            model.AutofillTarget2 = secondary;
            model.AutofillTarget2Current = 0;
            model.AutofillTarget2Goal = 300;

            double before = GetWeightedDeficit(model);

            model.OptimizeForGoals();

            double after = GetWeightedDeficit(model);
            Assert.True(after <= before + 0.000001d, $"Expected objective to stay the same or improve. Before={before}, After={after}");
        }

        [Fact]
        public void OptimizeForGoals_ImprovesSingleAgilityGoalWhenAvailable()
        {
            BuildModel model = CreateHighStatsBuild();
            model.ResetAutofillClusters();

            Assert.Contains("Agility", model.AutofillTargetOptions);

            model.AutofillTarget1 = "Agility";
            model.AutofillTarget1Current = 0;
            model.AutofillTarget1Goal = 200;

            double before = GetWeightedDeficit(model);

            model.OptimizeForGoals();

            double after = GetWeightedDeficit(model);
            Assert.True(after < before, $"Expected optimization to reduce deficit. Before={before}, After={after}");
        }

        [Fact]
        public void OptimizeForGoals_DoesNotClearSelectionsOnTiedObjective()
        {
            BuildModel model = CreateHighStatsBuild();
            model.ResetAutofillClusters();

            ImplantModel implantWithAgilityShiny = model.Implants
                .First(i => i.ShinyClusterOptions.Contains("Agility"));
            implantWithAgilityShiny.ShinyClusterSelection = "Agility";

            model.AutofillTarget1 = "Agility";
            model.AutofillTarget1Current = 500;
            model.AutofillTarget1Goal = 1;

            model.OptimizeForGoals();

            Assert.Equal("Agility", implantWithAgilityShiny.ShinyClusterSelection);
        }

        [Fact]
        public void OptimizeForGoals_RespectsPriorityTradeoffs()
        {
            BuildModel agilityPriorityModel = CreateHighStatsBuild();
            agilityPriorityModel.ResetAutofillClusters();
            Assert.Contains("Agility", agilityPriorityModel.AutofillTargetOptions);
            Assert.Contains("Treatment", agilityPriorityModel.AutofillTargetOptions);

            agilityPriorityModel.AutofillTarget1 = "Agility";
            agilityPriorityModel.AutofillTarget1Current = 0;
            agilityPriorityModel.AutofillTarget1Goal = 400;
            agilityPriorityModel.AutofillTarget2 = "Treatment";
            agilityPriorityModel.AutofillTarget2Current = 0;
            agilityPriorityModel.AutofillTarget2Goal = 400;
            agilityPriorityModel.OptimizeForGoals();

            BuildModel treatmentPriorityModel = CreateHighStatsBuild();
            treatmentPriorityModel.ResetAutofillClusters();
            treatmentPriorityModel.AutofillTarget1 = "Treatment";
            treatmentPriorityModel.AutofillTarget1Current = 0;
            treatmentPriorityModel.AutofillTarget1Goal = 400;
            treatmentPriorityModel.AutofillTarget2 = "Agility";
            treatmentPriorityModel.AutofillTarget2Current = 0;
            treatmentPriorityModel.AutofillTarget2Goal = 400;
            treatmentPriorityModel.OptimizeForGoals();

            double agilityGainWhenPriority1 = GetProjection(agilityPriorityModel, "Agility", 1).TotalImplantDrivenGain;
            double agilityGainWhenPriority2 = GetProjection(treatmentPriorityModel, "Agility", 2).TotalImplantDrivenGain;
            double treatmentGainWhenPriority1 = GetProjection(treatmentPriorityModel, "Treatment", 1).TotalImplantDrivenGain;
            double treatmentGainWhenPriority2 = GetProjection(agilityPriorityModel, "Treatment", 2).TotalImplantDrivenGain;

            Assert.True(
                agilityGainWhenPriority1 >= agilityGainWhenPriority2,
                $"Expected Agility to receive at least as much implant gain when it is priority 1. P1={agilityGainWhenPriority1}, P2={agilityGainWhenPriority2}");
            Assert.True(
                treatmentGainWhenPriority1 >= treatmentGainWhenPriority2,
                $"Expected Treatment to receive at least as much implant gain when it is priority 1. P1={treatmentGainWhenPriority1}, P2={treatmentGainWhenPriority2}");
        }

        [Fact]
        public void OptimizeForGoals_CanBeatNaiveSinglePassLocalSearch()
        {
            const string primaryTarget = "Treatment";
            const string secondaryTarget = "Agility";

            BuildModel naiveModel = CreateHighStatsBuild();
            Assert.Contains(primaryTarget, naiveModel.AutofillTargetOptions);
            Assert.Contains(secondaryTarget, naiveModel.AutofillTargetOptions);

            ConfigureGoals(naiveModel, primaryTarget, secondaryTarget, 500, 500);
            RunNaiveSinglePassOptimizer(naiveModel);
            double naiveObjective = GetWeightedDeficit(naiveModel);

            BuildModel enhancedModel = CreateHighStatsBuild();
            ConfigureGoals(enhancedModel, primaryTarget, secondaryTarget, 500, 500);
            enhancedModel.OptimizeForGoals();
            double enhancedObjective = GetWeightedDeficit(enhancedModel);

            Assert.True(
                enhancedObjective <= naiveObjective + 0.000001d,
                $"Expected enhanced optimizer to be at least as good as naive single-pass local search on a challenging goal set. naive={naiveObjective}, enhanced={enhancedObjective}");
        }

        private static BuildModel CreateHighStatsBuild()
        {
            BuildModel model = new BuildModel
            {
                AutofillTarget1 = string.Empty,
                AutofillTarget2 = string.Empty,
                AutofillTarget3 = string.Empty
            };

            model.Stats.Strength = 1200;
            model.Stats.Agility = 1200;
            model.Stats.Stamina = 1200;
            model.Stats.Intelligence = 1200;
            model.Stats.Sense = 1200;
            model.Stats.Psychic = 1200;
            model.Stats.Treatment = 3000;
            return model;
        }

        private static double GetWeightedDeficit(BuildModel model)
        {
            return model.TargetProjections.Sum(p => p.Priority switch
            {
                1 => 9d * p.DeficitToGoal,
                2 => 3d * p.DeficitToGoal,
                _ => 1d * p.DeficitToGoal
            });
        }

        private static BuildModel.TargetProjectionModel GetProjection(BuildModel model, string target, int priority)
        {
            BuildModel.TargetProjectionModel projection = model.TargetProjections
                .Single(p => p.Priority == priority && p.Target == target);

            return projection;
        }

        private static void ConfigureGoals(BuildModel model, string target1, string target2, double goal1, double goal2)
        {
            model.ResetAutofillClusters();
            model.AutofillTarget1 = target1;
            model.AutofillTarget1Current = 0;
            model.AutofillTarget1Goal = goal1;
            model.AutofillTarget2 = target2;
            model.AutofillTarget2Current = 0;
            model.AutofillTarget2Goal = goal2;
            model.AutofillTarget3 = string.Empty;
            model.AutofillTarget3Current = 0;
            model.AutofillTarget3Goal = 0;
        }

        private static void RunNaiveSinglePassOptimizer(BuildModel model)
        {
            foreach (ImplantModel implant in model.Implants.Where(i => !i.IsUnavailable))
            {
                OptimizeSelectionSinglePass(
                    model,
                    implant.ShinyClusterOptions,
                    getSelection: () => implant.ShinyClusterSelection,
                    setSelection: selection => implant.ShinyClusterSelection = selection);

                OptimizeSelectionSinglePass(
                    model,
                    implant.BrightClusterOptions,
                    getSelection: () => implant.BrightClusterSelection,
                    setSelection: selection => implant.BrightClusterSelection = selection);

                OptimizeSelectionSinglePass(
                    model,
                    implant.FadedClusterOptions,
                    getSelection: () => implant.FadedClusterSelection,
                    setSelection: selection => implant.FadedClusterSelection = selection);
            }
        }

        private static void OptimizeSelectionSinglePass(
            BuildModel model,
            System.Collections.Generic.IEnumerable<string> options,
            Func<string> getSelection,
            Action<string> setSelection)
        {
            string original = getSelection() ?? string.Empty;
            string best = original;
            double bestObjective = GetWeightedDeficit(model);

            foreach (string option in EnumerateNaiveOptions(original, options))
            {
                setSelection(option);
                double objective = GetWeightedDeficit(model);

                if (objective < bestObjective - 0.000001d)
                {
                    best = option;
                    bestObjective = objective;
                }
            }

            setSelection(best);
        }

        private static System.Collections.Generic.IEnumerable<string> EnumerateNaiveOptions(
            string currentSelection,
            System.Collections.Generic.IEnumerable<string> options)
        {
            yield return currentSelection;

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
    }
}
