using AOLadderer.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AOLadderer.UnitTests.Stats
{
    [TestClass]
    public class SkillTests
    {
        [TestMethod]
        public void GetShinySkillIncreases()
        {
            Assert.AreEqual(6, Skill.Rifle.GetShinyStatIncrease(1));
            Assert.AreEqual(55, Skill.Rifle.GetShinyStatIncrease(99));
            Assert.AreEqual(55, Skill.Rifle.GetShinyStatIncrease(100));
            Assert.AreEqual(56, Skill.Rifle.GetShinyStatIncrease(101));
            Assert.AreEqual(105, Skill.Rifle.GetShinyStatIncrease(200));
        }

        [TestMethod]
        public void GetBrightSkillIncreases()
        {
            Assert.AreEqual(3, Skill.Rifle.GetBrightStatIncrease(1));
            Assert.AreEqual(30, Skill.Rifle.GetBrightStatIncrease(89));
            Assert.AreEqual(30, Skill.Rifle.GetBrightStatIncrease(90));
            Assert.AreEqual(30, Skill.Rifle.GetBrightStatIncrease(91));
            Assert.AreEqual(30, Skill.Rifle.GetBrightStatIncrease(92));
            Assert.AreEqual(31, Skill.Rifle.GetBrightStatIncrease(93));
            Assert.AreEqual(63, Skill.Rifle.GetBrightStatIncrease(200));
        }

        [TestMethod]
        public void GetFadedSkillIncreases()
        {
            Assert.AreEqual(2, Skill.Rifle.GetFadedStatIncrease(1));
            Assert.AreEqual(38, Skill.Rifle.GetFadedStatIncrease(179));
            Assert.AreEqual(38, Skill.Rifle.GetFadedStatIncrease(180));
            Assert.AreEqual(38, Skill.Rifle.GetFadedStatIncrease(181));
            Assert.AreEqual(38, Skill.Rifle.GetFadedStatIncrease(182));
            Assert.AreEqual(39, Skill.Rifle.GetFadedStatIncrease(183));
            Assert.AreEqual(42, Skill.Rifle.GetFadedStatIncrease(200));
        }

        [TestMethod]
        public void SkillCount()
            => Assert.AreEqual(67, Skill.Skills.Count);

        [TestMethod]
        public void TreatmentTrickledownMatchesExistingWeights()
        {
            Assert.AreEqual(1.5, Skill.Treatment.GetTrickledownGain(Ability.Agility, 20));
            Assert.AreEqual(5, Skill.Treatment.GetTrickledownGain(Ability.Intelligence, 40));
            Assert.AreEqual(1, Skill.Treatment.GetTrickledownGain(Ability.Sense, 20));
            Assert.AreEqual(0, Skill.Treatment.GetTrickledownGain(Ability.Strength, 20));
        }

        [TestMethod]
        public void MartialArtsTrickledownUsesConfiguredWeights()
        {
            double totalGain = Skill.MartialArts.GetTrickledownGain(Ability.Strength, 20)
                + Skill.MartialArts.GetTrickledownGain(Ability.Agility, 40)
                + Skill.MartialArts.GetTrickledownGain(Ability.Psychic, 8);

            Assert.AreEqual(6.6, totalGain, 0.0001);
        }

        [TestMethod]
        public void SkillDependencyTotalsAreEitherUnspecifiedOrComplete()
        {
            foreach (Skill skill in Skill.Skills)
            {
                double totalWeight = skill.Dependency.AbilityWeights.Values.Sum();
                bool isValid = Math.Abs(totalWeight) < 0.000001d || Math.Abs(totalWeight - 1d) < 0.000001d;

                Assert.IsTrue(isValid, $"{skill.Name} has invalid dependency total: {totalWeight}.");

                foreach (double weight in skill.Dependency.AbilityWeights.Values)
                {
                    Assert.IsTrue(weight >= 0d && weight <= 1d, $"{skill.Name} has out-of-range weight: {weight}.");
                }
            }
        }

        [TestMethod]
        public void SpotChecksCorePlanningSkillDependencies()
        {
            AssertDependency(
                Skill.MartialArts,
                (Ability.Strength, 0.2d),
                (Ability.Agility, 0.5d),
                (Ability.Psychic, 0.3d));

            AssertDependency(
                Skill.Rifle,
                (Ability.Agility, 0.6d),
                (Ability.Sense, 0.4d));

            AssertDependency(
                Skill.AimedShot,
                (Ability.Sense, 1d));

            AssertDependency(
                Skill.CompLiter,
                (Ability.Intelligence, 1d));
        }

        [TestMethod]
        public void PracticalPhase2SkillsMatchExpectedMappings()
        {
            IReadOnlyDictionary<Skill, (Ability Ability, double Weight)[]> expectedMappings
                = new Dictionary<Skill, (Ability Ability, double Weight)[]>
            {
                [Skill.AssaultRif] = new[] { (Ability.Strength, 0.3d), (Ability.Agility, 0.7d) },
                [Skill.Burst] = new[] { (Ability.Agility, 0.6d), (Ability.Stamina, 0.4d) },
                [Skill.FullAuto] = new[] { (Ability.Strength, 0.3d), (Ability.Agility, 0.7d) },
                [Skill.DodgeRng] = new[] { (Ability.Agility, 0.5d), (Ability.Intelligence, 0.2d), (Ability.Sense, 0.3d) },
                [Skill.EvadeClsC] = new[] { (Ability.Agility, 0.5d), (Ability.Intelligence, 0.2d), (Ability.Sense, 0.3d) },
                [Skill.DuckExp] = new[] { (Ability.Agility, 0.5d), (Ability.Intelligence, 0.2d), (Ability.Sense, 0.3d) },
                [Skill.BioMetamor] = new[] { (Ability.Intelligence, 0.8d), (Ability.Psychic, 0.2d) },
                [Skill.MattMetam] = new[] { (Ability.Intelligence, 0.8d), (Ability.Psychic, 0.2d) },
                [Skill.MatterCrea] = new[] { (Ability.Intelligence, 0.8d), (Ability.Psychic, 0.2d) },
                [Skill.PsychoModi] = new[] { (Ability.Intelligence, 0.8d), (Ability.Psychic, 0.2d) },
                [Skill.SensoryImpr] = new[] { (Ability.Intelligence, 0.8d), (Ability.Sense, 0.2d) },
                [Skill.TimeAndSpace] = new[] { (Ability.Intelligence, 0.8d), (Ability.Psychic, 0.2d) },
                [Skill.ElecEngi] = new[] { (Ability.Intelligence, 1d) },
                [Skill.MechEngi] = new[] { (Ability.Intelligence, 1d) },
                [Skill.NanoProgra] = new[] { (Ability.Intelligence, 1d) },
                [Skill.WeaponSmt] = new[] { (Ability.Strength, 0.2d), (Ability.Agility, 0.3d), (Ability.Intelligence, 0.5d) }
            };

            foreach (KeyValuePair<Skill, (Ability Ability, double Weight)[]> entry in expectedMappings)
            {
                AssertDependency(entry.Key, entry.Value);
            }
        }

        private static void AssertDependency(Skill skill, params (Ability Ability, double Weight)[] expected)
        {
            IReadOnlyDictionary<Ability, double> weights = skill.Dependency.AbilityWeights;
            Assert.AreEqual(expected.Length, weights.Count, $"Unexpected dependency count for {skill.Name}.");

            foreach ((Ability ability, double weight) in expected)
            {
                Assert.IsTrue(weights.TryGetValue(ability, out double actualWeight), $"Missing {ability.Name} dependency for {skill.Name}.");
                Assert.AreEqual(weight, actualWeight, 0.000001d, $"Unexpected weight for {skill.Name} ({ability.Name}).");
            }
        }
    }
}
