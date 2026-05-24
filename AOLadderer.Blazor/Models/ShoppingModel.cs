using System.Collections.Generic;
using System.Linq;

namespace AOLadderer.Blazor.Models
{
    public class ShoppingModel
    {
        public class StageModel
        {
            public StageModel(string name, IEnumerable<Implant> implants)
            {
                Name = name;
                ShinyClusters = implants
                    .Where(i => i.ShinyClusterTemplate != null)
                    .Select(i => new ClusterModel(i.MinimumShinyClusterQL.Value, i.ShinyStat))
                    .OrderBy(c => c.Stat).ThenBy(c => c.MinimumQL)
                    .ToArray();
                BrightClusters = implants
                    .Where(i => i.BrightClusterTemplate != null)
                    .Select(i => new ClusterModel(i.MinimumBrightClusterQL.Value, i.BrightStat))
                    .OrderBy(c => c.Stat).ThenBy(c => c.MinimumQL)
                    .ToArray();
                FadedClusters = implants
                    .Where(i => i.FadedClusterTemplate != null)
                    .Select(i => new ClusterModel(i.MinimumFadedClusterQL.Value, i.FadedStat))
                    .OrderBy(c => c.Stat).ThenBy(c => c.MinimumQL)
                    .ToArray();
            }

            public string Name { get; }
            public IReadOnlyCollection<ClusterModel> ShinyClusters { get; }
            public IReadOnlyCollection<ClusterModel> BrightClusters { get; }
            public IReadOnlyCollection<ClusterModel> FadedClusters { get; }
            public bool HasAnyClusters => ShinyClusters.Any() || BrightClusters.Any() || FadedClusters.Any();
        }

        public class ClusterModel
        {
            public ClusterModel(int minimumQL, Stat stat)
            {
                MinimumQL = minimumQL;
                Stat = stat.Name;
            }

            public int MinimumQL { get; }
            public string Stat { get; }
            public bool IsChecked { get; set; }
        }

        public ShoppingModel(LadderProcess ladder)
        {
            LadderStage = new StageModel("Temporary ladder implants", ladder.OrderedLadderImplants);
            FinalStage = new StageModel("Final implants", ladder.OrderedFinalImplants);
        }

        public StageModel LadderStage { get; }
        public StageModel FinalStage { get; }
    }
}
