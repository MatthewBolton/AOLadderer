using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AOLadderer.Blazor.Models
{
    public class LadderModel : IReadOnlyCollection<LadderModel.StepModel>
    {
        public class StepModel
        {
            public StepModel(Implant implant, bool isFinalImplant, int stepNumber)
            {
                StepNumber = stepNumber;
                ImplantQL = implant.QL;
                ImplantSlot = implant.ImplantSlot?.Name;
                ShinyStat = implant.ShinyStat?.Name;
                BrightStat = implant.BrightStat?.Name;
                FadedStat = implant.FadedStat?.Name;
                IsFinalImplant = isFinalImplant;
            }

            public int StepNumber { get; }
            public int ImplantQL { get; }
            public string ImplantSlot { get; }
            public string ShinyStat { get; }
            public string BrightStat { get; }
            public string FadedStat { get; }
            public bool IsFinalImplant { get; }
            public bool IsBuilt { get; set; }
            public bool IsEquipped { get; set; }
            public bool WasCommandCopied { get; set; }
        }

        private readonly IReadOnlyCollection<StepModel> steps;

        public LadderModel(LadderProcess ladder)
        {
            int stepNumber = 1;
            LadderSteps = ladder.OrderedLadderImplants
                .Select(i => new StepModel(i, isFinalImplant: false, stepNumber: stepNumber++))
                .ToArray();
            FinalSteps = ladder.OrderedFinalImplants
                .Select(i => new StepModel(i, isFinalImplant: true, stepNumber: stepNumber++))
                .ToArray();
            steps = LadderSteps
                .Concat(FinalSteps)
                .ToArray();
            AverageFinalImplantQL = ladder.AverageFinalImplantQL;
        }

        public IReadOnlyCollection<StepModel> LadderSteps { get; }
        public IReadOnlyCollection<StepModel> FinalSteps { get; }
        public double AverageFinalImplantQL { get; }

        public int Count => steps.Count;
        public IEnumerator<StepModel> GetEnumerator() => steps.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => steps.GetEnumerator();
    }
}
