using System;
using System.Collections.Generic;
using System.Linq;

namespace AOLadderer.Blazor.Models
{
    public sealed class AoPresetSourceRow
    {
        public AoPresetSourceRow(string profession, string variant, string slotLabel, string gradeLabel, string clusterLabel)
        {
            Profession = profession ?? string.Empty;
            Variant = variant ?? string.Empty;
            SlotLabel = slotLabel ?? string.Empty;
            GradeLabel = gradeLabel ?? string.Empty;
            ClusterLabel = clusterLabel ?? string.Empty;
        }

        public string Profession { get; }
        public string Variant { get; }
        public string SlotLabel { get; }
        public string GradeLabel { get; }
        public string ClusterLabel { get; }
    }

    public sealed class AoPresetSlotSelection
    {
        public AoPresetSlotSelection(ImplantSlot slot, bool hasShiny, string shiny, bool hasBright, string bright, bool hasFaded, string faded)
        {
            Slot = slot;
            HasShiny = hasShiny;
            ShinyCluster = shiny ?? string.Empty;
            HasBright = hasBright;
            BrightCluster = bright ?? string.Empty;
            HasFaded = hasFaded;
            FadedCluster = faded ?? string.Empty;
        }

        public ImplantSlot Slot { get; }
        public bool HasShiny { get; }
        public string ShinyCluster { get; }
        public bool HasBright { get; }
        public string BrightCluster { get; }
        public bool HasFaded { get; }
        public string FadedCluster { get; }
    }

    public sealed class AoNormalizedPreset
    {
        public AoNormalizedPreset(string id, string profession, string variant, IReadOnlyList<AoPresetSlotSelection> slotSelections)
        {
            Id = id ?? string.Empty;
            Profession = profession ?? string.Empty;
            Variant = variant ?? string.Empty;
            SlotSelections = slotSelections ?? Array.Empty<AoPresetSlotSelection>();
        }

        public string Id { get; }
        public string Profession { get; }
        public string Variant { get; }
        public IReadOnlyList<AoPresetSlotSelection> SlotSelections { get; }
        public string DisplayName => string.IsNullOrWhiteSpace(Variant) ? Profession : $"{Profession} - {Variant}";
    }

    public static class AoUniversePresetCatalog
    {
        private sealed class SlotSelectionBuilder
        {
            public SlotSelectionBuilder(ImplantSlot slot)
                => Slot = slot;

            public ImplantSlot Slot { get; }
            public bool HasShiny { get; private set; }
            public string ShinyCluster { get; private set; } = string.Empty;
            public bool HasBright { get; private set; }
            public string BrightCluster { get; private set; } = string.Empty;
            public bool HasFaded { get; private set; }
            public string FadedCluster { get; private set; } = string.Empty;

            public void SetCluster(ClusterGrade grade, string cluster)
            {
                switch (grade)
                {
                    case ClusterGrade.Shiny:
                        HasShiny = true;
                        ShinyCluster = cluster;
                        break;
                    case ClusterGrade.Bright:
                        HasBright = true;
                        BrightCluster = cluster;
                        break;
                    default:
                        HasFaded = true;
                        FadedCluster = cluster;
                        break;
                }
            }

            public AoPresetSlotSelection Build()
                => new AoPresetSlotSelection(Slot, HasShiny, ShinyCluster, HasBright, BrightCluster, HasFaded, FadedCluster);
        }

        private static readonly IReadOnlyList<AoPresetSourceRow> SourceRows = new[]
        {
            // ── ADVENTURER ────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Adventurer", "", "Eye",        "Bright", "Heavy Weapons"),
            new AoPresetSourceRow("Adventurer", "", "Eye",        "Faded",  "Matter Creation"),
            new AoPresetSourceRow("Adventurer", "", "Head",       "Shiny",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Adventurer", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Adventurer", "", "Ear",        "Shiny",  "Perception"),
            new AoPresetSourceRow("Adventurer", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Adventurer", "", "Ear",        "Faded",  "Intelligence"),
            new AoPresetSourceRow("Adventurer", "", "Right Arm",  "Shiny",  "1h Edged Weapon"),
            new AoPresetSourceRow("Adventurer", "", "Right Arm",  "Bright", "Brawling"),
            new AoPresetSourceRow("Adventurer", "", "Right Arm",  "Faded",  "Fast Attack"),
            new AoPresetSourceRow("Adventurer", "", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Adventurer", "", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Adventurer", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Adventurer", "", "Left Arm",   "Shiny",  "Brawling"),
            new AoPresetSourceRow("Adventurer", "", "Left Arm",   "Bright", "Strength"),
            new AoPresetSourceRow("Adventurer", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Adventurer", "", "Right Wrist","Shiny",  "Parry"),
            new AoPresetSourceRow("Adventurer", "", "Right Wrist","Bright", "1h Edged Weapon"),
            new AoPresetSourceRow("Adventurer", "", "Right Wrist","Faded",  "Multi Melee"),
            new AoPresetSourceRow("Adventurer", "", "Waist",      "Shiny",  "Chemical AC"),
            new AoPresetSourceRow("Adventurer", "", "Waist",      "Bright", "Sense"),
            new AoPresetSourceRow("Adventurer", "", "Waist",      "Faded",  "Agility"),
            new AoPresetSourceRow("Adventurer", "", "Left Wrist", "Shiny",  "Multi Melee"),
            new AoPresetSourceRow("Adventurer", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Adventurer", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Adventurer", "", "Right Hand", "Bright", "Fast Attack"),
            new AoPresetSourceRow("Adventurer", "", "Right Hand", "Faded",  "1h Edged Weapon"),
            new AoPresetSourceRow("Adventurer", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Adventurer", "", "Leg",        "Bright", "Melee Initiative"),
            new AoPresetSourceRow("Adventurer", "", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Adventurer", "", "Left Hand",  "Shiny",  "Fast Attack"),
            new AoPresetSourceRow("Adventurer", "", "Left Hand",  "Bright", "Trap Disarm"),
            new AoPresetSourceRow("Adventurer", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Adventurer", "", "Feet",       "Shiny",  "Melee Initiative"),
            new AoPresetSourceRow("Adventurer", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Adventurer", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── AGENT ─────────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Agent", "", "Eye",        "Shiny",  "Rifle"),
            new AoPresetSourceRow("Agent", "", "Eye",        "Bright", "Psychological Modifications"),
            new AoPresetSourceRow("Agent", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Agent", "", "Head",       "Shiny",  "Sensory Improvement"),
            new AoPresetSourceRow("Agent", "", "Head",       "Bright", "Ranged Initiative"),
            new AoPresetSourceRow("Agent", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Agent", "", "Ear",        "Shiny",  "Perception"),
            new AoPresetSourceRow("Agent", "", "Ear",        "Bright", "Concealment"),
            new AoPresetSourceRow("Agent", "", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Agent", "", "Right Arm",  "Shiny",  "Fling Shot"),
            new AoPresetSourceRow("Agent", "", "Right Arm",  "Faded",  "Mechanical Engineering"),
            new AoPresetSourceRow("Agent", "", "Chest",      "Shiny",  "Nano Pool"),
            new AoPresetSourceRow("Agent", "", "Chest",      "Bright", "Biological Metamorphosis"),
            new AoPresetSourceRow("Agent", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Agent", "", "Left Arm",   "Bright", "Break & Entry"),
            new AoPresetSourceRow("Agent", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Agent", "", "Right Wrist","Shiny",  "Ranged Initiative"),
            new AoPresetSourceRow("Agent", "", "Right Wrist","Bright", "Rifle"),
            new AoPresetSourceRow("Agent", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Agent", "", "Waist",      "Bright", "Duck Explosives"),
            new AoPresetSourceRow("Agent", "", "Waist",      "Faded",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Agent", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Agent", "", "Left Wrist", "Faded",  "Rifle"),
            new AoPresetSourceRow("Agent", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Agent", "", "Right Hand", "Faded",  "Computer Literacy"),
            new AoPresetSourceRow("Agent", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Agent", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Agent", "", "Left Hand",  "Bright", "Trap Disarm"),
            new AoPresetSourceRow("Agent", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Agent", "", "Feet",       "Shiny",  "Concealment"),
            new AoPresetSourceRow("Agent", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Agent", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── BUREAUCRAT ────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Bureaucrat", "", "Eye",        "Shiny",  "Electrical Engineering"),
            new AoPresetSourceRow("Bureaucrat", "", "Eye",        "Bright", "Psychological Modifications"),
            new AoPresetSourceRow("Bureaucrat", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Bureaucrat", "", "Head",       "Shiny",  "Matter Creation"),
            new AoPresetSourceRow("Bureaucrat", "", "Head",       "Bright", "Nano Pool"),
            new AoPresetSourceRow("Bureaucrat", "", "Head",       "Faded",  "Tutoring"),
            new AoPresetSourceRow("Bureaucrat", "", "Ear",        "Bright", "Psychology"),
            new AoPresetSourceRow("Bureaucrat", "", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Arm",  "Shiny",  "Shotgun"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Arm",  "Bright", "Chemical AC"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Arm",  "Faded",  "Mechanical Engineering"),
            new AoPresetSourceRow("Bureaucrat", "", "Chest",      "Shiny",  "Nano Pool"),
            new AoPresetSourceRow("Bureaucrat", "", "Chest",      "Bright", "Biological Metamorphosis"),
            new AoPresetSourceRow("Bureaucrat", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Arm",   "Bright", "Break & Entry"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Wrist","Shiny",  "Ranged Initiative"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Wrist","Bright", "Nano Resist"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Bureaucrat", "", "Waist",      "Shiny",  "Radiation AC"),
            new AoPresetSourceRow("Bureaucrat", "", "Waist",      "Bright", "Max Nano"),
            new AoPresetSourceRow("Bureaucrat", "", "Waist",      "Faded",  "Shotgun"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Wrist", "Shiny",  "Multi Ranged"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Bureaucrat", "", "Right Hand", "Faded",  "Treatment"),
            new AoPresetSourceRow("Bureaucrat", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Bureaucrat", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Bureaucrat", "", "Leg",        "Faded",  "Body Development"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Bureaucrat", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Bureaucrat", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Bureaucrat", "", "Feet",       "Bright", "Dodge Ranged"),
            new AoPresetSourceRow("Bureaucrat", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── DOCTOR ────────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Doctor", "", "Eye",        "Bright", "Treatment"),
            new AoPresetSourceRow("Doctor", "", "Eye",        "Faded",  "Matter Creation"),
            new AoPresetSourceRow("Doctor", "", "Head",       "Shiny",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Doctor", "", "Head",       "Bright", "Nano Pool"),
            new AoPresetSourceRow("Doctor", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Doctor", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Doctor", "", "Ear",        "Faded",  "Intelligence"),
            new AoPresetSourceRow("Doctor", "", "Right Arm",  "Shiny",  "Break & Entry"),
            new AoPresetSourceRow("Doctor", "", "Right Arm",  "Bright", "Chemical AC"),
            new AoPresetSourceRow("Doctor", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Doctor", "", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Doctor", "", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Doctor", "", "Chest",      "Faded",  "Max Nano"),
            new AoPresetSourceRow("Doctor", "", "Left Arm",   "Bright", "Strength"),
            new AoPresetSourceRow("Doctor", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Doctor", "", "Right Wrist","Shiny",  "Pistol"),
            new AoPresetSourceRow("Doctor", "", "Right Wrist","Bright", "Nano Resist"),
            new AoPresetSourceRow("Doctor", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Doctor", "", "Waist",      "Shiny",  "Chemical AC"),
            new AoPresetSourceRow("Doctor", "", "Waist",      "Bright", "Max Nano"),
            new AoPresetSourceRow("Doctor", "", "Waist",      "Faded",  "Stamina"),
            new AoPresetSourceRow("Doctor", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Doctor", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Doctor", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Doctor", "", "Right Hand", "Faded",  "Treatment"),
            new AoPresetSourceRow("Doctor", "", "Leg",        "Shiny",  "Dodge Ranged"),
            new AoPresetSourceRow("Doctor", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Doctor", "", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Doctor", "", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Doctor", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Doctor", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Doctor", "", "Feet",       "Bright", "Dodge Ranged"),
            new AoPresetSourceRow("Doctor", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── ENFORCER (1h Blunt) ───────────────────────────────────────────────────
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Eye",        "Bright", "Sensory Improvement"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Eye",        "Faded",  "Matter Creation"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Head",       "Shiny",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Ear",        "Bright", "Concealment"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Arm",  "Shiny",  "1h Blunt"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Arm",  "Bright", "Brawling"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Arm",  "Faded",  "Fast Attack"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Chest",      "Faded",  "2h Blunt"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Arm",   "Shiny",  "Brawling"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Arm",   "Bright", "2h Blunt"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Wrist","Shiny",  "Run Speed"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Wrist","Bright", "1h Blunt"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Wrist","Faded",  "Multi Melee"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Waist",      "Shiny",  "Cold AC"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Waist",      "Bright", "Max Health"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Waist",      "Faded",  "Brawling"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Wrist", "Shiny",  "Multi Melee"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Hand", "Bright", "Matter Creation"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Right Hand", "Faded",  "1h Blunt"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Leg",        "Shiny",  "Dodge Ranged"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Hand",  "Shiny",  "Fast Attack"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Left Hand",  "Faded",  "Cold AC"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Enforcer", "1h Blunt", "Feet",       "Faded",  "Duck Explosives"),

            // ── ENFORCER (2h Blunt) ───────────────────────────────────────────────────
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Eye",        "Bright", "Sensory Improvement"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Eye",        "Faded",  "Matter Creation"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Head",       "Shiny",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Ear",        "Bright", "Concealment"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Arm",  "Shiny",  "2h Blunt"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Arm",  "Bright", "Brawling"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Arm",  "Faded",  "Fast Attack"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Chest",      "Faded",  "2h Blunt"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Arm",   "Shiny",  "Brawling"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Arm",   "Bright", "2h Blunt"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Wrist","Shiny",  "Run Speed"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Wrist","Bright", "Nano Resist"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Wrist","Faded",  "Multi Melee"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Waist",      "Shiny",  "Cold AC"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Waist",      "Bright", "Max Health"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Waist",      "Faded",  "Brawling"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Wrist", "Shiny",  "Multi Melee"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Hand", "Bright", "Matter Creation"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Right Hand", "Faded",  "Treatment"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Leg",        "Shiny",  "Dodge Ranged"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Hand",  "Shiny",  "Fast Attack"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Left Hand",  "Faded",  "Cold AC"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Enforcer", "2h Blunt", "Feet",       "Faded",  "Duck Explosives"),

            // ── ENGINEER ──────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Engineer", "", "Eye",        "Shiny",  "Tutoring"),
            new AoPresetSourceRow("Engineer", "", "Eye",        "Bright", "Intelligence"),
            new AoPresetSourceRow("Engineer", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Engineer", "", "Head",       "Shiny",  "Matter Creation"),
            new AoPresetSourceRow("Engineer", "", "Head",       "Bright", "Nano Pool"),
            new AoPresetSourceRow("Engineer", "", "Head",       "Faded",  "Tutoring"),
            new AoPresetSourceRow("Engineer", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Engineer", "", "Ear",        "Faded",  "Intelligence"),
            new AoPresetSourceRow("Engineer", "", "Right Arm",  "Shiny",  "MG / SMG"),
            new AoPresetSourceRow("Engineer", "", "Right Arm",  "Bright", "Chemical AC"),
            new AoPresetSourceRow("Engineer", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Engineer", "", "Chest",      "Shiny",  "Nano Pool"),
            new AoPresetSourceRow("Engineer", "", "Chest",      "Bright", "Biological Metamorphosis"),
            new AoPresetSourceRow("Engineer", "", "Chest",      "Faded",  "NanoC. Init"),
            new AoPresetSourceRow("Engineer", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Engineer", "", "Right Wrist","Shiny",  "Pistol"),
            new AoPresetSourceRow("Engineer", "", "Right Wrist","Bright", "Nano Resist"),
            new AoPresetSourceRow("Engineer", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Engineer", "", "Waist",      "Bright", "Max Nano"),
            new AoPresetSourceRow("Engineer", "", "Waist",      "Faded",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Engineer", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Engineer", "", "Right Hand", "Bright", "Matter Creation"),
            new AoPresetSourceRow("Engineer", "", "Right Hand", "Faded",  "Treatment"),
            new AoPresetSourceRow("Engineer", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Engineer", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Engineer", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Engineer", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Engineer", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Engineer", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── FIXER ─────────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Fixer", "", "Eye",        "Shiny",  "Tutoring"),
            new AoPresetSourceRow("Fixer", "", "Eye",        "Bright", "Sensory Improvement"),
            new AoPresetSourceRow("Fixer", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Fixer", "", "Head",       "Shiny",  "Psychic"),
            new AoPresetSourceRow("Fixer", "", "Head",       "Bright", "Nano Pool"),
            new AoPresetSourceRow("Fixer", "", "Head",       "Faded",  "Tutoring"),
            new AoPresetSourceRow("Fixer", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Fixer", "", "Ear",        "Faded",  "Psychic"),
            new AoPresetSourceRow("Fixer", "", "Right Arm",  "Shiny",  "MG / SMG"),
            new AoPresetSourceRow("Fixer", "", "Right Arm",  "Bright", "Chemical AC"),
            new AoPresetSourceRow("Fixer", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Fixer", "", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Fixer", "", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Fixer", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Fixer", "", "Left Arm",   "Bright", "Break & Entry"),
            new AoPresetSourceRow("Fixer", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Fixer", "", "Right Wrist","Shiny",  "Run Speed"),
            new AoPresetSourceRow("Fixer", "", "Right Wrist","Bright", "Burst"),
            new AoPresetSourceRow("Fixer", "", "Waist",      "Shiny",  "Fire AC"),
            new AoPresetSourceRow("Fixer", "", "Waist",      "Bright", "Max Health"),
            new AoPresetSourceRow("Fixer", "", "Waist",      "Faded",  "Stamina"),
            new AoPresetSourceRow("Fixer", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Fixer", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Fixer", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Fixer", "", "Right Hand", "Faded",  "Burst"),
            new AoPresetSourceRow("Fixer", "", "Leg",        "Shiny",  "Dodge Ranged"),
            new AoPresetSourceRow("Fixer", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Fixer", "", "Leg",        "Faded",  "Run Speed"),
            new AoPresetSourceRow("Fixer", "", "Left Hand",  "Bright", "Trap Disarm"),
            new AoPresetSourceRow("Fixer", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Fixer", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Fixer", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Fixer", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── KEEPER ────────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Keeper", "", "Eye",        "Bright", "Psychological Modifications"),
            new AoPresetSourceRow("Keeper", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Keeper", "", "Head",       "Shiny",  "Sensory Improvement"),
            new AoPresetSourceRow("Keeper", "", "Head",       "Bright", "Dimach"),
            new AoPresetSourceRow("Keeper", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Keeper", "", "Ear",        "Bright", "Psychology"),
            new AoPresetSourceRow("Keeper", "", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Keeper", "", "Right Arm",  "Shiny",  "2h Edged"),
            new AoPresetSourceRow("Keeper", "", "Right Arm",  "Bright", "Brawling"),
            new AoPresetSourceRow("Keeper", "", "Right Arm",  "Faded",  "Fast Attack"),
            new AoPresetSourceRow("Keeper", "", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Keeper", "", "Chest",      "Bright", "Biological Metamorphosis"),
            new AoPresetSourceRow("Keeper", "", "Chest",      "Faded",  "Strength"),
            new AoPresetSourceRow("Keeper", "", "Left Arm",   "Shiny",  "Brawling"),
            new AoPresetSourceRow("Keeper", "", "Left Arm",   "Bright", "2h Edged"),
            new AoPresetSourceRow("Keeper", "", "Right Wrist","Shiny",  "Parry"),
            new AoPresetSourceRow("Keeper", "", "Waist",      "Bright", "Max Health"),
            new AoPresetSourceRow("Keeper", "", "Waist",      "Faded",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Keeper", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Keeper", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Keeper", "", "Right Hand", "Faded",  "Treatment"),
            new AoPresetSourceRow("Keeper", "", "Leg",        "Shiny",  "Dodge Ranged"),
            new AoPresetSourceRow("Keeper", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Keeper", "", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Keeper", "", "Left Hand",  "Shiny",  "Fast Attack"),
            new AoPresetSourceRow("Keeper", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Keeper", "", "Feet",       "Shiny",  "Melee Initiative"),
            new AoPresetSourceRow("Keeper", "", "Feet",       "Bright", "Dodge Ranged"),

            // ── MARTIAL ARTIST ────────────────────────────────────────────────────────
            new AoPresetSourceRow("Martial Artist", "", "Eye",        "Shiny",  "Tutoring"),
            new AoPresetSourceRow("Martial Artist", "", "Eye",        "Bright", "Psychological Modifications"),
            new AoPresetSourceRow("Martial Artist", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Martial Artist", "", "Head",       "Shiny",  "First Aid"),
            new AoPresetSourceRow("Martial Artist", "", "Head",       "Bright", "Dimach"),
            new AoPresetSourceRow("Martial Artist", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Martial Artist", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Martial Artist", "", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Martial Artist", "", "Right Arm",  "Shiny",  "Strength"),
            new AoPresetSourceRow("Martial Artist", "", "Right Arm",  "Bright", "Physical Initiative"),
            new AoPresetSourceRow("Martial Artist", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Martial Artist", "", "Chest",      "Shiny",  "Dimach"),
            new AoPresetSourceRow("Martial Artist", "", "Chest",      "Bright", "Biological Metamorphosis"),
            new AoPresetSourceRow("Martial Artist", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Martial Artist", "", "Left Arm",   "Shiny",  "Brawling"),
            new AoPresetSourceRow("Martial Artist", "", "Left Arm",   "Bright", "Strength"),
            new AoPresetSourceRow("Martial Artist", "", "Left Arm",   "Faded",  "Physical Initiative"),
            new AoPresetSourceRow("Martial Artist", "", "Right Wrist","Shiny",  "Riposte"),
            new AoPresetSourceRow("Martial Artist", "", "Right Wrist","Bright", "Nano Resist"),
            new AoPresetSourceRow("Martial Artist", "", "Waist",      "Shiny",  "Radiation AC"),
            new AoPresetSourceRow("Martial Artist", "", "Waist",      "Bright", "Max Health"),
            new AoPresetSourceRow("Martial Artist", "", "Waist",      "Faded",  "Evade Close"),
            new AoPresetSourceRow("Martial Artist", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Martial Artist", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Martial Artist", "", "Right Hand", "Shiny",  "Martial Arts"),
            new AoPresetSourceRow("Martial Artist", "", "Right Hand", "Bright", "First Aid"),
            new AoPresetSourceRow("Martial Artist", "", "Right Hand", "Faded",  "Treatment"),
            new AoPresetSourceRow("Martial Artist", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Martial Artist", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Martial Artist", "", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Martial Artist", "", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Martial Artist", "", "Left Hand",  "Faded",  "Martial Arts"),
            new AoPresetSourceRow("Martial Artist", "", "Feet",       "Shiny",  "Physical Initiative"),
            new AoPresetSourceRow("Martial Artist", "", "Feet",       "Bright", "Martial Arts"),
            new AoPresetSourceRow("Martial Artist", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── META PHYSICIST ────────────────────────────────────────────────────────
            new AoPresetSourceRow("Meta Physicist", "", "Eye",        "Shiny",  "Tutoring"),
            new AoPresetSourceRow("Meta Physicist", "", "Eye",        "Bright", "Psychological Modifications"),
            new AoPresetSourceRow("Meta Physicist", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Meta Physicist", "", "Head",       "Shiny",  "Matter Creation"),
            new AoPresetSourceRow("Meta Physicist", "", "Head",       "Bright", "Nano Pool"),
            new AoPresetSourceRow("Meta Physicist", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Meta Physicist", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Meta Physicist", "", "Ear",        "Faded",  "Intelligence"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Arm",  "Shiny",  "1h Blunt"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Arm",  "Bright", "Chemical AC"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Meta Physicist", "", "Chest",      "Shiny",  "Nano Pool"),
            new AoPresetSourceRow("Meta Physicist", "", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Meta Physicist", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Arm",   "Bright", "Break & Entry"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Wrist","Shiny",  "Run Speed"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Wrist","Bright", "1h Blunt"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Wrist","Faded",  "Multi Melee"),
            new AoPresetSourceRow("Meta Physicist", "", "Waist",      "Shiny",  "Chemical AC"),
            new AoPresetSourceRow("Meta Physicist", "", "Waist",      "Bright", "Max Nano"),
            new AoPresetSourceRow("Meta Physicist", "", "Waist",      "Faded",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Wrist", "Shiny",  "Multi Melee"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Meta Physicist", "", "Right Hand", "Faded",  "1h Blunt"),
            new AoPresetSourceRow("Meta Physicist", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Meta Physicist", "", "Leg",        "Bright", "Stamina"),
            new AoPresetSourceRow("Meta Physicist", "", "Leg",        "Faded",  "Run Speed"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Meta Physicist", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Meta Physicist", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Meta Physicist", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Meta Physicist", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── NANO-TECHNICIAN ───────────────────────────────────────────────────────
            new AoPresetSourceRow("Nano-Technician", "", "Eye",        "Shiny",  "Tutoring"),
            new AoPresetSourceRow("Nano-Technician", "", "Eye",        "Bright", "Computer Literacy"),
            new AoPresetSourceRow("Nano-Technician", "", "Eye",        "Faded",  "Matter Creation"),
            new AoPresetSourceRow("Nano-Technician", "", "Head",       "Shiny",  "Matter Creation"),
            new AoPresetSourceRow("Nano-Technician", "", "Head",       "Bright", "Nano Pool"),
            new AoPresetSourceRow("Nano-Technician", "", "Ear",        "Shiny",  "Perception"),
            new AoPresetSourceRow("Nano-Technician", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Nano-Technician", "", "Ear",        "Faded",  "Intelligence"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Arm",  "Shiny",  "Assault Rif"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Nano-Technician", "", "Chest",      "Shiny",  "Nano Pool"),
            new AoPresetSourceRow("Nano-Technician", "", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Nano-Technician", "", "Chest",      "Faded",  "Sensory Improvement"),
            new AoPresetSourceRow("Nano-Technician", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Wrist","Shiny",  "Pistol"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Wrist","Bright", "Nano Resist"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Nano-Technician", "", "Waist",      "Bright", "Max Nano"),
            new AoPresetSourceRow("Nano-Technician", "", "Waist",      "Faded",  "Biological Metamorphosis"),
            new AoPresetSourceRow("Nano-Technician", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Nano-Technician", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Hand", "Bright", "Matter Creation"),
            new AoPresetSourceRow("Nano-Technician", "", "Right Hand", "Faded",  "Computer Literacy"),
            new AoPresetSourceRow("Nano-Technician", "", "Leg",        "Shiny",  "Dodge Ranged"),
            new AoPresetSourceRow("Nano-Technician", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Nano-Technician", "", "Leg",        "Faded",  "Body Development"),
            new AoPresetSourceRow("Nano-Technician", "", "Left Hand",  "Bright", "Trap Disarm"),
            new AoPresetSourceRow("Nano-Technician", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Nano-Technician", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Nano-Technician", "", "Feet",       "Bright", "Dodge Ranged"),
            new AoPresetSourceRow("Nano-Technician", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── SOLDIER ───────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Soldier", "", "Eye",        "Shiny",  "Aimed Shot"),
            new AoPresetSourceRow("Soldier", "", "Eye",        "Bright", "Ranged Ener"),
            new AoPresetSourceRow("Soldier", "", "Eye",        "Faded",  "Assault Rif"),
            new AoPresetSourceRow("Soldier", "", "Head",       "Shiny",  "Ranged Ener"),
            new AoPresetSourceRow("Soldier", "", "Head",       "Bright", "Ranged. Init"),
            new AoPresetSourceRow("Soldier", "", "Ear",        "Bright", "Concealment"),
            new AoPresetSourceRow("Soldier", "", "Ear",        "Faded",  "Psychological Modifications"),
            new AoPresetSourceRow("Soldier", "", "Right Arm",  "Shiny",  "Assault Rif"),
            new AoPresetSourceRow("Soldier", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Soldier", "", "Chest",      "Shiny",  "Max Health"),
            new AoPresetSourceRow("Soldier", "", "Chest",      "Bright", "Imp/Proj AC"),
            new AoPresetSourceRow("Soldier", "", "Chest",      "Faded",  "Max Nano"),
            new AoPresetSourceRow("Soldier", "", "Left Arm",   "Bright", "Strength"),
            new AoPresetSourceRow("Soldier", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Soldier", "", "Right Wrist","Shiny",  "Run Speed"),
            new AoPresetSourceRow("Soldier", "", "Right Wrist","Bright", "Burst"),
            new AoPresetSourceRow("Soldier", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Soldier", "", "Waist",      "Shiny",  "Cold AC"),
            new AoPresetSourceRow("Soldier", "", "Waist",      "Bright", "Max Health"),
            new AoPresetSourceRow("Soldier", "", "Waist",      "Faded",  "Stamina"),
            new AoPresetSourceRow("Soldier", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Soldier", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Soldier", "", "Right Hand", "Bright", "Assault Rif"),
            new AoPresetSourceRow("Soldier", "", "Right Hand", "Faded",  "Burst"),
            new AoPresetSourceRow("Soldier", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Soldier", "", "Leg",        "Bright", "Stamina"),
            new AoPresetSourceRow("Soldier", "", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Soldier", "", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Soldier", "", "Left Hand",  "Faded",  "Ranged Ener"),
            new AoPresetSourceRow("Soldier", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Soldier", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Soldier", "", "Feet",       "Faded",  "Duck Explosives"),

            // ── TRADER ────────────────────────────────────────────────────────────────
            new AoPresetSourceRow("Trader", "", "Eye",        "Bright", "Psychological Modifications"),
            new AoPresetSourceRow("Trader", "", "Eye",        "Faded",  "Time & Space"),
            new AoPresetSourceRow("Trader", "", "Head",       "Shiny",  "Psychological Modifications"),
            new AoPresetSourceRow("Trader", "", "Head",       "Bright", "Ranged. Init"),
            new AoPresetSourceRow("Trader", "", "Head",       "Faded",  "Sense"),
            new AoPresetSourceRow("Trader", "", "Ear",        "Shiny",  "Perception"),
            new AoPresetSourceRow("Trader", "", "Ear",        "Bright", "Tutoring"),
            new AoPresetSourceRow("Trader", "", "Ear",        "Faded",  "Intelligence"),
            new AoPresetSourceRow("Trader", "", "Right Arm",  "Shiny",  "Shotgun"),
            new AoPresetSourceRow("Trader", "", "Right Arm",  "Bright", "Chemical AC"),
            new AoPresetSourceRow("Trader", "", "Right Arm",  "Faded",  "Radiation AC"),
            new AoPresetSourceRow("Trader", "", "Chest",      "Shiny",  "Stamina"),
            new AoPresetSourceRow("Trader", "", "Chest",      "Bright", "Matter Metamorphosis"),
            new AoPresetSourceRow("Trader", "", "Chest",      "Faded",  "Strength"),
            new AoPresetSourceRow("Trader", "", "Left Arm",   "Bright", "Strength"),
            new AoPresetSourceRow("Trader", "", "Left Arm",   "Faded",  "Matter Metamorphosis"),
            new AoPresetSourceRow("Trader", "", "Right Wrist","Shiny",  "Ranged. Init"),
            new AoPresetSourceRow("Trader", "", "Right Wrist","Bright", "Burst"),
            new AoPresetSourceRow("Trader", "", "Right Wrist","Faded",  "Fling Shot"),
            new AoPresetSourceRow("Trader", "", "Waist",      "Shiny",  "Chemical AC"),
            new AoPresetSourceRow("Trader", "", "Waist",      "Bright", "Max Nano"),
            new AoPresetSourceRow("Trader", "", "Waist",      "Faded",  "Shotgun"),
            new AoPresetSourceRow("Trader", "", "Left Wrist", "Shiny",  "Multi Ranged"),
            new AoPresetSourceRow("Trader", "", "Left Wrist", "Bright", "Run Speed"),
            new AoPresetSourceRow("Trader", "", "Left Wrist", "Faded",  "Nano Resist"),
            new AoPresetSourceRow("Trader", "", "Right Hand", "Shiny",  "Trap Disarm"),
            new AoPresetSourceRow("Trader", "", "Right Hand", "Bright", "Time & Space"),
            new AoPresetSourceRow("Trader", "", "Right Hand", "Faded",  "Computer Literacy"),
            new AoPresetSourceRow("Trader", "", "Leg",        "Shiny",  "Agility"),
            new AoPresetSourceRow("Trader", "", "Leg",        "Bright", "Evade Close"),
            new AoPresetSourceRow("Trader", "", "Leg",        "Faded",  "Max Health"),
            new AoPresetSourceRow("Trader", "", "Left Hand",  "Bright", "Fire AC"),
            new AoPresetSourceRow("Trader", "", "Left Hand",  "Faded",  "First Aid"),
            new AoPresetSourceRow("Trader", "", "Feet",       "Shiny",  "Evade Close"),
            new AoPresetSourceRow("Trader", "", "Feet",       "Bright", "Agility"),
            new AoPresetSourceRow("Trader", "", "Feet",       "Faded",  "Duck Explosives"),
        };

        private static readonly IReadOnlyDictionary<string, ImplantSlot> SlotByNormalizedLabel = BuildSlotLookup();
        private static readonly IReadOnlyDictionary<string, ClusterGrade> GradeByNormalizedLabel = new Dictionary<string, ClusterGrade>(StringComparer.Ordinal)
        {
            ["shiny"]  = ClusterGrade.Shiny,
            ["shinny"] = ClusterGrade.Shiny,
            ["s"]      = ClusterGrade.Shiny,
            ["bright"] = ClusterGrade.Bright,
            ["brigt"]  = ClusterGrade.Bright,
            ["brite"]  = ClusterGrade.Bright,
            ["b"]      = ClusterGrade.Bright,
            ["faded"]  = ClusterGrade.Faded,
            ["fadded"] = ClusterGrade.Faded,
            ["fadad"]  = ClusterGrade.Faded,
            ["f"]      = ClusterGrade.Faded
        };

        // Maps a normalized input label to the normalized form of the actual stat display name,
        // when they differ (e.g. "Biological Metamorphosis" → normalized key for "Bio.Metamor").
        private static readonly IReadOnlyDictionary<string, string> ClusterNormalizationAliases = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Bio.Metamor
            ["biometa"]                    = "biometamor",
            ["biometamorphosis"]           = "biometamor",
            ["biologicalmetamorphosis"]    = "biometamor",
            // Matt.Metam
            ["mattermetamorphosis"]        = "mattmetam",
            ["mattmetamorphosis"]          = "mattmetam",
            // Matter Crea
            ["mattercreation"]             = "mattercrea",
            ["mc"]                         = "mattercrea",
            // Sensory Impr
            ["sensoryimprovement"]         = "sensoryimpr",
            ["si"]                         = "sensoryimpr",
            // Time & Space — "Time & Space" itself normalizes to "timespace" which is the correct key.
            // "Time and Space" (variant spelling) needs remapping.
            ["timeandspace"]               = "timespace",
            ["ts"]                         = "timespace",
            // Psycho Modi
            ["psychologicalmodification"]  = "psychomodi",
            ["psychologicalmodifications"] = "psychomodi",
            ["psychomodifications"]        = "psychomodi",
            ["pm"]                         = "psychomodi",
            // MG / SMG
            ["mgsmg"]                      = "mgsmg",
            // Aimed Shot
            ["aimedshot"]                  = "aimedshot",
            // 1h Edged Weapon
            ["1hedged"]                    = "1hedgedweapon",
            // Multi Melee (Mult. Melee)
            ["multimelee"]                 = "multmelee",
            // Melee. Init
            ["meleeinitiative"]            = "meleeinit",
            // Duck-Exp
            ["duckexplosives"]             = "duckexp",
            // Evade-ClsC
            ["evadeclose"]                 = "evadeclsc",
            // Dodge-Rng
            ["dodgeranged"]                = "dodgerng",
            // Body Dev
            ["bodydevelopment"]            = "bodydev",
            // Assault Rif
            ["assaultrifle"]               = "assaultrif",
            // Ranged. Init
            ["rangedinitiative"]           = "rangedinit",
            // Physic. Init
            ["physicalinitiative"]         = "physicinit",
            // NanoC. Init
            ["nanocinitiative"]            = "nanocinit",
            // Riposte (AO Universe sometimes has "Riposite" typo)
            ["riposite"]                   = "riposte",
            // Ranged Ener
            ["rangedenergy"]               = "rangedener",
            // Comp. Liter
            ["computerliteracy"]           = "compliter",
            // Fling Shot
            ["fling"]                      = "flingshot",
            // Elec. Engi
            ["electricalengineering"]      = "elecengi",
            // Mech. Engi
            ["mechanicalengineering"]      = "mechengi",
            // Imp/Proj AC
            ["projectilac"]                = "impprojac",
            ["projectileac"]               = "impprojac",
        };

        private static readonly IReadOnlyList<AoNormalizedPreset> Presets = BuildPresets();
        private static readonly IReadOnlyDictionary<string, AoNormalizedPreset> PresetById = Presets
            .ToDictionary(p => p.Id, p => p, StringComparer.Ordinal);
        private static readonly IReadOnlyCollection<string> ProfessionOptionsInternal = Presets
            .Select(p => p.Profession)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(p => p)
            .ToArray();

        public static IReadOnlyList<AoNormalizedPreset> AllPresets => Presets;
        public static IReadOnlyCollection<string> ProfessionOptions => ProfessionOptionsInternal;

        public static IReadOnlyList<AoNormalizedPreset> GetPresetsForProfession(string profession)
        {
            if (string.IsNullOrWhiteSpace(profession)) return Array.Empty<AoNormalizedPreset>();

            return Presets
                .Where(p => p.Profession.Equals(profession, StringComparison.Ordinal))
                .OrderBy(p => p.Variant)
                .ToArray();
        }

        public static bool TryGetPresetById(string presetId, out AoNormalizedPreset preset)
        {
            preset = null;

            return !string.IsNullOrWhiteSpace(presetId)
                && PresetById.TryGetValue(presetId, out preset);
        }

        private static IReadOnlyDictionary<string, ImplantSlot> BuildSlotLookup()
        {
            var slotLookup = new Dictionary<string, ImplantSlot>(StringComparer.Ordinal);

            foreach (ImplantSlot slot in ImplantSlot.ImplantSlots)
            {
                AddSlotAlias(slotLookup, slot.Name, slot);
                AddSlotAlias(slotLookup, slot.ShortName, slot);
                AddSlotAlias(slotLookup, slot.Name.Replace("-", string.Empty), slot);
            }

            AddSlotAlias(slotLookup, "r arm",    ImplantSlot.RightArm);
            AddSlotAlias(slotLookup, "l arm",    ImplantSlot.LeftArm);
            AddSlotAlias(slotLookup, "r wrist",  ImplantSlot.RightWrist);
            AddSlotAlias(slotLookup, "l wrist",  ImplantSlot.LeftWrist);
            AddSlotAlias(slotLookup, "r hand",   ImplantSlot.RightHand);
            AddSlotAlias(slotLookup, "l hand",   ImplantSlot.LeftHand);
            AddSlotAlias(slotLookup, "rightarm",  ImplantSlot.RightArm);
            AddSlotAlias(slotLookup, "leftarm",   ImplantSlot.LeftArm);
            AddSlotAlias(slotLookup, "rightwrist", ImplantSlot.RightWrist);
            AddSlotAlias(slotLookup, "leftwrist",  ImplantSlot.LeftWrist);
            AddSlotAlias(slotLookup, "righthand",  ImplantSlot.RightHand);
            AddSlotAlias(slotLookup, "lefthand",   ImplantSlot.LeftHand);

            return slotLookup;
        }

        private static void AddSlotAlias(IDictionary<string, ImplantSlot> lookup, string alias, ImplantSlot slot)
        {
            string key = NormalizeLabel(alias);
            if (key.Length == 0) return;

            lookup[key] = slot;
        }

        private static IReadOnlyList<AoNormalizedPreset> BuildPresets()
        {
            var presets = new List<AoNormalizedPreset>();

            foreach (IGrouping<string, AoPresetSourceRow> groupedRows in SourceRows
                .Where(r => !string.IsNullOrWhiteSpace(r.Profession))
                .GroupBy(GetPresetGroupKey))
            {
                AoPresetSourceRow representative = groupedRows.First();
                string profession = representative.Profession.Trim();
                string variant = string.IsNullOrWhiteSpace(representative.Variant)
                    ? "Standard"
                    : representative.Variant.Trim();

                var builders = new Dictionary<ImplantSlot, SlotSelectionBuilder>();

                foreach (AoPresetSourceRow row in groupedRows)
                {
                    if (!TryResolveRow(row, out ImplantSlot slot, out ClusterGrade grade, out string cluster))
                    {
                        continue;
                    }

                    if (!builders.TryGetValue(slot, out SlotSelectionBuilder builder))
                    {
                        builder = new SlotSelectionBuilder(slot);
                        builders[slot] = builder;
                    }

                    builder.SetCluster(grade, cluster);
                }

                string id = $"{NormalizeLabel(profession)}:{NormalizeLabel(variant)}";
                AoPresetSlotSelection[] slotSelections = builders.Values
                    .Select(b => b.Build())
                    .OrderBy(s => s.Slot.Name)
                    .ToArray();

                presets.Add(new AoNormalizedPreset(id, profession, variant, slotSelections));
            }

            return presets
                .OrderBy(p => p.Profession)
                .ThenBy(p => p.Variant)
                .ToArray();
        }

        private static string GetPresetGroupKey(AoPresetSourceRow row)
            => $"{NormalizeLabel(row.Profession)}|{NormalizeLabel(string.IsNullOrWhiteSpace(row.Variant) ? "standard" : row.Variant)}";

        private static bool TryResolveRow(AoPresetSourceRow row, out ImplantSlot slot, out ClusterGrade grade, out string cluster)
        {
            cluster = string.Empty;

            bool hasSlot  = TryResolveSlot(row.SlotLabel, out slot);
            bool hasGrade = TryResolveGrade(row.GradeLabel, out grade);

            if (!hasSlot || !hasGrade)
            {
                return false;
            }

            return TryResolveCluster(slot, grade, row.ClusterLabel, out cluster);
        }

        private static bool TryResolveSlot(string rawSlot, out ImplantSlot slot)
            => SlotByNormalizedLabel.TryGetValue(NormalizeLabel(rawSlot), out slot);

        private static bool TryResolveGrade(string rawGrade, out ClusterGrade grade)
            => GradeByNormalizedLabel.TryGetValue(NormalizeLabel(rawGrade), out grade);

        private static bool TryResolveCluster(ImplantSlot slot, ClusterGrade grade, string rawCluster, out string cluster)
        {
            cluster = string.Empty;

            ImplantModel implant = new ImplantModel(slot);
            IReadOnlyCollection<string> options = grade switch
            {
                ClusterGrade.Shiny  => implant.ShinyClusterOptions,
                ClusterGrade.Bright => implant.BrightClusterOptions,
                _                   => implant.FadedClusterOptions
            };

            IReadOnlyDictionary<string, string> optionsByNormalizedLabel = options
                .ToDictionary(option => NormalizeLabel(option), option => option, StringComparer.Ordinal);

            string normalizedCluster = NormalizeLabel(rawCluster);
            if (ClusterNormalizationAliases.TryGetValue(normalizedCluster, out string aliasTarget))
            {
                normalizedCluster = aliasTarget;
            }

            return optionsByNormalizedLabel.TryGetValue(normalizedCluster, out cluster);
        }

        private static string NormalizeLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            return new string(value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }
    }
}
