using System.Collections.Generic;
using System.Linq;
using AOLadderer;
using AOLadderer.Blazor.Models;
using Xunit;

namespace AOLadderer.Blazor.UnitTests
{
    public class ShopBuyableQLTests
    {
        [Theory]
        [InlineData(0,   0)]
        [InlineData(4,   0)]
        [InlineData(5,   5)]
        [InlineData(9,   5)]
        [InlineData(10,  10)]
        [InlineData(19,  10)]
        [InlineData(20,  20)]
        [InlineData(29,  20)]
        [InlineData(30,  30)]
        [InlineData(39,  30)]
        [InlineData(40,  40)]
        [InlineData(44,  40)]   // the example in the user's report: head ql 44 → must round to 40
        [InlineData(49,  40)]
        [InlineData(50,  50)]
        [InlineData(51,  51)]
        [InlineData(123, 123)]
        [InlineData(199, 199)]
        [InlineData(200, 200)]
        [InlineData(250, 200)]
        public void ClampToShopBuyableQL_RoundsDownToValidBreaks(int input, int expected)
        {
            Assert.Equal(expected, Implant.ClampToShopBuyableQL(input));
        }

        [Fact]
        public void LadderProcess_ShopMode_OnlyProducesShopBuyableQLs()
        {
            BuildModel model = CreateLowLevelTwinkBuild();
            model.ShopBuyableQLMode = true;

            LadderProcess ladder = model.CreateLadderProcess();

            foreach (Implant implant in ladder.OrderedLadderImplants.Concat(ladder.OrderedFinalImplants))
            {
                Assert.True(IsShopBuyableQL(implant.QL),
                    $"Ladder produced QL {implant.QL} (slot {implant.ImplantSlot.Name}) which Implantbob would reject.");
            }
        }

        [Fact]
        public void LadderProcess_ShopMode_NoOpAboveQL50()
        {
            // High-stats build pushes all implants well above 50, so the clamp should be a no-op.
            BuildModel highBuild = CreateHighStatsBuild();
            highBuild.ShopBuyableQLMode = false;
            LadderProcess unclamped = highBuild.CreateLadderProcess();

            BuildModel highBuildClamped = CreateHighStatsBuild();
            highBuildClamped.ShopBuyableQLMode = true;
            LadderProcess clamped = highBuildClamped.CreateLadderProcess();

            Assert.Equal(unclamped.TotalFinalImplantQL, clamped.TotalFinalImplantQL);
        }

        [Fact]
        public void Permalink_PreservesShopBuyableQLMode()
        {
            BuildModel original = CreateLowLevelTwinkBuild();
            original.ShopBuyableQLMode = true;

            BuildModel restored = new BuildModel();
            restored.UrlTokenDeserialize(SerializeAsUrlTokens(original));

            Assert.True(restored.ShopBuyableQLMode);
        }

        [Fact]
        public void Permalink_OldLinksWithoutFlagDefaultToOff()
        {
            // Simulate an old permalink: serialize, then drop the trailing shop-mode token.
            BuildModel original = CreateLowLevelTwinkBuild();
            Queue<object> tokens = SerializeAsUrlTokens(original);
            Queue<object> truncated = new Queue<object>(tokens.Take(tokens.Count - 1));

            BuildModel restored = new BuildModel();
            restored.UrlTokenDeserialize(truncated);

            Assert.False(restored.ShopBuyableQLMode);
        }

        // The real permalink pipeline routes values through string serialization (URL encoding).
        // Mirror that here so test round-trips behave like production round-trips.
        private static Queue<object> SerializeAsUrlTokens(BuildModel model)
        {
            Queue<object> raw = new Queue<object>();
            model.UrlTokenSerialize(raw);
            return new Queue<object>(raw.Select(token =>
            {
                string s = System.Convert.ToString(token) ?? string.Empty;
                // Enum.ToString gives the member name (e.g. "None"); convert to its numeric value.
                if (token is System.Enum e)
                {
                    s = System.Convert.ToInt32(e).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                return (object)s;
            }));
        }

        private static bool IsShopBuyableQL(int ql)
            => ql == 5 || ql == 10 || ql == 20 || ql == 30 || ql == 40 || (ql >= 50 && ql <= 200);

        private static BuildModel CreateLowLevelTwinkBuild()
        {
            BuildModel model = new BuildModel();
            model.Stats.Strength = 90;
            model.Stats.Agility = 90;
            model.Stats.Stamina = 90;
            model.Stats.Intelligence = 90;
            model.Stats.Sense = 90;
            model.Stats.Psychic = 90;
            model.Stats.Treatment = 200;
            // Apply a profession preset so the ladder process has final implants to chew on.
            model.SetPresetProfession("Doctor");
            model.ApplySelectedPreset();
            return model;
        }

        private static BuildModel CreateHighStatsBuild()
        {
            BuildModel model = new BuildModel();
            model.Stats.Strength = 1200;
            model.Stats.Agility = 1200;
            model.Stats.Stamina = 1200;
            model.Stats.Intelligence = 1200;
            model.Stats.Sense = 1200;
            model.Stats.Psychic = 1200;
            model.Stats.Treatment = 3000;
            model.SetPresetProfession("Doctor");
            model.ApplySelectedPreset();
            return model;
        }
    }
}
