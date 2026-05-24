using System.Linq;
using AOLadderer.Blazor.Models;
using Xunit;

namespace AOLadderer.Blazor.UnitTests
{
    public class BuildModelPresetTests
    {
        [Fact]
        public void ApplySelectedPreset_DoctorStandardSetsExpectedClusters()
        {
            BuildModel model = new BuildModel();
            SelectPreset(model, "Doctor", "Standard");

            model.ApplySelectedPreset();

            Assert.Equal("Bio.Metamor",  GetImplant(model, ImplantSlot.Head).ShinyClusterSelection);
            Assert.Equal("Treatment",    GetImplant(model, ImplantSlot.Eye).BrightClusterSelection);
            Assert.Equal("Matter Crea",  GetImplant(model, ImplantSlot.Eye).FadedClusterSelection);
            Assert.Equal("Time & Space", GetImplant(model, ImplantSlot.RightHand).BrightClusterSelection);
            Assert.Equal("Treatment",    GetImplant(model, ImplantSlot.RightHand).FadedClusterSelection);
            Assert.Equal("Dodge-Rng",    GetImplant(model, ImplantSlot.Leg).ShinyClusterSelection);
            Assert.Equal("Evade-ClsC",   GetImplant(model, ImplantSlot.Feet).ShinyClusterSelection);
            Assert.Equal("Chemical AC",  GetImplant(model, ImplantSlot.Waist).ShinyClusterSelection);
        }

        [Fact]
        public void ApplySelectedPreset_LeavesClustersEditable()
        {
            BuildModel model = new BuildModel();
            SelectPreset(model, "Doctor", "Standard");
            model.ApplySelectedPreset();

            ImplantModel head = GetImplant(model, ImplantSlot.Head);
            head.ShinyClusterSelection = "Intelligence";

            Assert.Equal("Intelligence", head.ShinyClusterSelection);
        }

        [Fact]
        public void ClearAppliedPresetClusters_OnlyClearsPresetTouchedGrades()
        {
            BuildModel model = new BuildModel();
            // Doctor Standard sets Left Arm Bright and Faded but NOT Left Arm Shiny.
            ImplantModel leftArm = GetImplant(model, ImplantSlot.LeftArm);
            leftArm.ShinyClusterSelection = "Brawling";

            SelectPreset(model, "Doctor", "Standard");
            model.ApplySelectedPreset();
            model.ClearAppliedPresetClusters();

            Assert.Equal("Brawling",   leftArm.ShinyClusterSelection);  // not touched by preset
            Assert.Equal(string.Empty, leftArm.BrightClusterSelection); // cleared by preset
            Assert.Equal(string.Empty, GetImplant(model, ImplantSlot.Head).ShinyClusterSelection);
            Assert.Equal(string.Empty, GetImplant(model, ImplantSlot.Eye).BrightClusterSelection);
        }

        [Fact]
        public void ApplySelectedPreset_AgentStandardSetsExpectedClusters()
        {
            BuildModel model = new BuildModel();
            SelectPreset(model, "Agent", "Standard");

            model.ApplySelectedPreset();

            Assert.Equal("Rifle",        GetImplant(model, ImplantSlot.Eye).ShinyClusterSelection);
            Assert.Equal("Sensory Impr", GetImplant(model, ImplantSlot.Head).ShinyClusterSelection);
            Assert.Equal("Perception",   GetImplant(model, ImplantSlot.Ear).ShinyClusterSelection);
            Assert.Equal("Ranged. Init", GetImplant(model, ImplantSlot.RightWrist).ShinyClusterSelection);
            Assert.Equal("Concealment",  GetImplant(model, ImplantSlot.Feet).ShinyClusterSelection);
        }

        [Fact]
        public void ApplySelectedPreset_DoesNotChangeUnavailableSlots()
        {
            BuildModel model = new BuildModel();
            ImplantModel head = GetImplant(model, ImplantSlot.Head);
            head.IsUnavailable = true;
            head.ShinyClusterSelection = "Matter Crea";

            SelectPreset(model, "Doctor", "Standard");
            model.ApplySelectedPreset();

            Assert.Equal("Matter Crea", head.ShinyClusterSelection);
        }

        [Fact]
        public void ClearAppliedPresetClusters_ClearsOnlyPresetAppliedGrades()
        {
            BuildModel model = new BuildModel();
            // Doctor Standard sets Eye Bright = Treatment but not Eye Shiny.
            ImplantModel eye = GetImplant(model, ImplantSlot.Eye);
            eye.ShinyClusterSelection = "Rifle";

            SelectPreset(model, "Doctor", "Standard");
            model.ApplySelectedPreset();
            model.ClearAppliedPresetClusters();

            Assert.Equal("Rifle",        eye.ShinyClusterSelection);
            Assert.Equal(string.Empty,   eye.BrightClusterSelection);
        }

        private static void SelectPreset(BuildModel model, string profession, string variant)
        {
            model.SetPresetProfession(profession);
            model.PresetId = model.PresetOptionsForSelectedProfession
                .Single(p => p.Variant == variant)
                .Id;
        }

        private static ImplantModel GetImplant(BuildModel model, ImplantSlot slot)
            => model.Implants.Single(i => i.Slot == slot);
    }
}
