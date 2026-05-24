using System.Collections.Generic;

namespace AOLadderer.Stats
{
    public sealed class SkillDependency
    {
        private readonly IReadOnlyDictionary<Ability, double> _abilityWeights;

        public SkillDependency(IReadOnlyDictionary<Ability, double> abilityWeights)
            => _abilityWeights = abilityWeights ?? new Dictionary<Ability, double>();

        public IReadOnlyDictionary<Ability, double> AbilityWeights => _abilityWeights;

        public bool HasAbilityWeights => _abilityWeights.Count > 0;

        public double GetTrickledownGain(Ability ability, double abilityGain)
            => ability != null && _abilityWeights.TryGetValue(ability, out double weight)
                ? weight * abilityGain / 4d
                : 0;

        public static readonly SkillDependency None = new SkillDependency(null);
    }

    public sealed class Skill : Stat
    {
        private static SkillDependency D(
            double strength = 0d,
            double agility = 0d,
            double stamina = 0d,
            double intelligence = 0d,
            double sense = 0d,
            double psychic = 0d)
        {
            if (strength == 0d
                && agility == 0d
                && stamina == 0d
                && intelligence == 0d
                && sense == 0d
                && psychic == 0d)
            {
                return SkillDependency.None;
            }

            var weights = new Dictionary<Ability, double>();
            if (strength != 0d) weights[Ability.Strength] = strength;
            if (agility != 0d) weights[Ability.Agility] = agility;
            if (stamina != 0d) weights[Ability.Stamina] = stamina;
            if (intelligence != 0d) weights[Ability.Intelligence] = intelligence;
            if (sense != 0d) weights[Ability.Sense] = sense;
            if (psychic != 0d) weights[Ability.Psychic] = psychic;

            return new SkillDependency(weights);
        }

        private Skill(string name, SkillDependency dependency = null)
            : base(name)
        {
            Dependency = dependency ?? SkillDependency.None;
        }

        public SkillDependency Dependency { get; }

        public double GetTrickledownGain(Ability ability, double abilityGain)
            => Dependency.GetTrickledownGain(ability, abilityGain);

        public override int GetShinyStatIncrease(int implantQL)
            => (int)(5.5025126 + 0.4974874 * implantQL + 0.5);

        public override int GetBrightStatIncrease(int implantQL)
            => (int)(2.6984925 + 0.3015075 * implantQL + 0.5);

        public override int GetFadedStatIncrease(int implantQL)
            => (int)(1.7989950 + 0.2010050 * implantQL + 0.5);

        public static readonly Skill
            OneHandBlunt = new Skill("1h Blunt", D(strength: 0.4d, agility: 0.3d, stamina: 0.3d)),
            OneHandEdgedWeapon = new Skill("1h Edged Weapon", D(strength: 0.3d, agility: 0.3d, sense: 0.4d)),
            TwoHandBlunt = new Skill("2h Blunt", D(strength: 0.5d, stamina: 0.5d)),
            TwoHandEdged = new Skill("2h Edged", D(strength: 0.6d, agility: 0.2d, stamina: 0.2d)),
            Adventuring = new Skill("Adventuring"),
            AimedShot = new Skill("Aimed Shot", D(sense: 1d)),
            AssaultRif = new Skill("Assault Rif", D(strength: 0.3d, agility: 0.7d)),
            BioMetamor = new Skill("Bio.Metamor", D(intelligence: 0.8d, psychic: 0.2d)),
            BodyDev = new Skill("Body Dev", D(stamina: 1d)),
            Bow = new Skill("Bow", D(agility: 0.8d, sense: 0.2d)),
            BowSpcAtt = new Skill("Bow Spc Att", D(agility: 0.8d, sense: 0.2d)),
            Brawling = new Skill("Brawling", D(strength: 0.6d, agility: 0.2d, stamina: 0.2d)),
            BreakAndEntry = new Skill("Break & Entry", D(agility: 1d)),
            Burst = new Skill("Burst", D(agility: 0.6d, stamina: 0.4d)),
            Chemistry = new Skill("Chemistry", D(intelligence: 0.8d, agility: 0.2d)),
            CompLiter = new Skill("Comp. Liter", D(intelligence: 1d)),
            Concealment = new Skill("Concealment", D(agility: 0.5d, sense: 0.5d)),
            Dimach = new Skill("Dimach", D(strength: 0.2d, psychic: 0.8d)),
            DodgeRng = new Skill("Dodge-Rng", D(agility: 0.5d, intelligence: 0.2d, sense: 0.3d)),
            DuckExp = new Skill("Duck-Exp", D(agility: 0.5d, intelligence: 0.2d, sense: 0.3d)),
            ElecEngi = new Skill("Elec. Engi", D(intelligence: 1d)),
            EvadeClsC = new Skill("Evade-ClsC", D(agility: 0.5d, intelligence: 0.2d, sense: 0.3d)),
            FastAttack = new Skill("Fast Attack", D(agility: 0.6d, sense: 0.4d)),
            FirstAid = new Skill("First Aid", D(agility: 0.4d, sense: 0.6d)),
            FlingShot = new Skill("Fling Shot", D(agility: 0.6d, sense: 0.4d)),
            FullAuto = new Skill("Full Auto", D(strength: 0.3d, agility: 0.7d)),
            Grenade = new Skill("Grenade", D(strength: 0.3d, agility: 0.5d, stamina: 0.2d)),
            HeavyWeapons = new Skill("Heavy Weapons", D(strength: 0.5d, agility: 0.2d, stamina: 0.3d)),
            MartialArts = new Skill("Martial Arts", D(strength: 0.2d, agility: 0.5d, psychic: 0.3d)),
            MattMetam = new Skill("Matt.Metam", D(intelligence: 0.8d, psychic: 0.2d)),
            MatterCrea = new Skill("Matter Crea", D(intelligence: 0.8d, psychic: 0.2d)),
            MechEngi = new Skill("Mech. Engi", D(intelligence: 1d)),
            MeleeEner = new Skill("Melee Ener", D(strength: 0.5d, agility: 0.5d)),
            MeleeInit = new Skill("Melee. Init", D(agility: 0.6d, sense: 0.4d)),
            MGSMG = new Skill("MG / SMG", D(agility: 0.6d, sense: 0.4d)),
            MultMelee = new Skill("Mult. Melee", D(strength: 0.4d, agility: 0.6d)),
            MultiRanged = new Skill("Multi Ranged", D(agility: 0.6d, sense: 0.4d)),
            NanoPool = new Skill("Nano Pool", D(psychic: 1d)),
            NanoProgra = new Skill("Nano Progra", D(intelligence: 1d)),
            NanoResist = new Skill("Nano Resist", D(intelligence: 0.2d, psychic: 0.8d)),
            NanoCInit = new Skill("NanoC. Init", D(intelligence: 0.6d, sense: 0.4d)),
            Parry = new Skill("Parry", D(strength: 0.5d, agility: 0.3d, sense: 0.2d)),
            Perception = new Skill("Perception", D(agility: 0.2d, intelligence: 0.3d, sense: 0.5d)),
            PharmaTech = new Skill("Pharma Tech", D(intelligence: 0.8d, agility: 0.2d)),
            PhysicInit = new Skill("Physic. Init", D(agility: 0.6d, sense: 0.4d)),
            Piercing = new Skill("Piercing", D(strength: 0.2d, agility: 0.5d, sense: 0.3d)),
            Pistol = new Skill("Pistol", D(agility: 0.6d, sense: 0.4d)),
            PsychoModi = new Skill("Psycho Modi", D(intelligence: 0.8d, psychic: 0.2d)),
            Psychology = new Skill("Psychology", D(intelligence: 0.5d, sense: 0.2d, psychic: 0.3d)),
            QuantumFT = new Skill("Quantum FT", D(intelligence: 1d)),
            RangedEner = new Skill("Ranged Ener", D(agility: 0.4d, intelligence: 0.4d, sense: 0.2d)),
            RangedInit = new Skill("Ranged. Init", D(agility: 0.6d, sense: 0.4d)),
            Rifle = new Skill("Rifle", D(agility: 0.6d, sense: 0.4d)),
            Riposte = new Skill("Riposte", D(strength: 0.5d, agility: 0.3d, sense: 0.2d)),
            RunSpeed = new Skill("Run Speed", D(agility: 1d)),
            SensoryImpr = new Skill("Sensory Impr", D(intelligence: 0.8d, sense: 0.2d)),
            SharpObj = new Skill("Sharp Obj", D(agility: 0.7d, sense: 0.3d)),
            Shotgun = new Skill("Shotgun", D(agility: 0.6d, sense: 0.4d)),
            SneakAtck = new Skill("Sneak Atck", D(agility: 0.4d, sense: 0.6d)),
            TimeAndSpace = new Skill("Time & Space", D(intelligence: 0.8d, psychic: 0.2d)),
            TrapDisarm = new Skill("Trap Disarm", D(agility: 0.7d, sense: 0.3d)),
            Treatment = new Skill("Treatment", D(agility: 0.3d, intelligence: 0.5d, sense: 0.2d)),
            Tutoring = new Skill("Tutoring", D(intelligence: 1d)),
            VehicleAir = new Skill("Vehicle Air"),
            VehicleGrnd = new Skill("Vehicle Grnd"),
            VehicleHydr = new Skill("Vehicle Hydr"),
            WeaponSmt = new Skill("Weapon Smt", D(strength: 0.2d, agility: 0.3d, intelligence: 0.5d));

        public static readonly IReadOnlyList<Skill> Skills = new[] { OneHandBlunt, OneHandEdgedWeapon, TwoHandBlunt, TwoHandEdged, Adventuring, AimedShot, AssaultRif, BioMetamor, BodyDev, Bow, BowSpcAtt, Brawling, BreakAndEntry, Burst, Chemistry, CompLiter, Concealment, Dimach, DodgeRng, DuckExp, ElecEngi, EvadeClsC, FastAttack, FirstAid, FlingShot, FullAuto, Grenade, HeavyWeapons, MartialArts, MattMetam, MatterCrea, MechEngi, MeleeEner, MeleeInit, MGSMG, MultMelee, MultiRanged, NanoPool, NanoProgra, NanoResist, NanoCInit, Parry, Perception, PharmaTech, PhysicInit, Piercing, Pistol, PsychoModi, Psychology, QuantumFT, RangedEner, RangedInit, Rifle, Riposte, RunSpeed, SensoryImpr, SharpObj, Shotgun, SneakAtck, TimeAndSpace, TrapDisarm, Treatment, Tutoring, VehicleAir, VehicleGrnd, VehicleHydr, WeaponSmt };
    }
}
