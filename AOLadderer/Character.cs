using AOLadderer.Stats;
using System.Collections.Generic;

namespace AOLadderer
{
    public class Character
    {
        private LadderStatValues _ladderStatValues;
        private ImplantConfiguration _implantConfiguration;

        public Character(
            int agilityValue, int intelligenceValue, int psychicValue, int senseValue,
            int staminaValue, int strengthValue, double treatmentValue,
            IEnumerable<Implant> implants = null)
        {
            _ladderStatValues = new LadderStatValues(
                agilityValue, intelligenceValue, psychicValue, senseValue,
                staminaValue, strengthValue, treatmentValue);
            _implantConfiguration = new ImplantConfiguration(implants);
        }

        public Character(
            IReadOnlyDictionary<Ability, int> abilityValues = null, double treatmentValue = 0,
            IEnumerable<Implant> implants = null)
        {
            _ladderStatValues = new LadderStatValues(abilityValues, treatmentValue);
            _implantConfiguration = new ImplantConfiguration(implants);
        }

        public Character(Character character)
        {
            _ladderStatValues = new LadderStatValues(character._ladderStatValues);
            _implantConfiguration = new ImplantConfiguration(character._implantConfiguration);
            ShopBuyableQLMode = character.ShopBuyableQLMode;
        }

        // When true, every implant QL chosen by GetMaxImplant / EquipMaxImplant is rounded down to the
        // nearest Implantbob-shop-buyable QL ({5,10,20,30,40} ∪ [50..200]). The ladder processes copy this
        // flag via the Character copy constructor, so all derived working characters inherit it.
        public bool ShopBuyableQLMode { get; set; }

        public int GetAbilityValue(Ability ability)
            => _ladderStatValues.GetAbilityValue(ability);

        public void SetAbilityValue(Ability ability, int abilityValue)
            => _ladderStatValues.SetAbilityValue(ability, abilityValue);

        public void IncreaseAbilityValue(Ability ability, int abilityIncrease)
            => _ladderStatValues.IncreaseAbilityValue(ability, abilityIncrease);

        public void DecreaseAbilityValue(Ability ability, int abilityDecrease)
            => _ladderStatValues.DecreaseAbilityValue(ability, abilityDecrease);

        public double TreatmentValue
        {
            get => _ladderStatValues.TreatmentValue;
            set => _ladderStatValues.TreatmentValue = value;
        }

        public Implant GetImplant(ImplantSlot implantSlot)
            => _implantConfiguration.GetImplant(implantSlot);

        public IEnumerable<Implant> GetEquippedImplants()
            => _implantConfiguration.GetEquippedImplants();

        public Implant UnequipImplant(ImplantSlot implantSlot)
        {
            var implant = GetImplant(implantSlot);

            if (implant != null)
            {
                _ladderStatValues.SubtractImplantValues(implant);
                _implantConfiguration.RemoveImplant(implantSlot);
            }

            return implant;
        }

        public void UnequipImplants(IEnumerable<ImplantSlot> implantSlots = null)
        {
            foreach (var implantSlot in implantSlots ?? ImplantSlot.ImplantSlots)
            {
                UnequipImplant(implantSlot);
            }
        }

        public void SetImplant(Implant implant, bool isSlotKnownToBeEmpty = false)
        {
            if (!isSlotKnownToBeEmpty)
            {
                UnequipImplant(implant.ImplantSlot);
            }

            _implantConfiguration.SetImplant(implant);
            _ladderStatValues.AddImplantValues(implant);
        }

        public void SetImplants(IEnumerable<Implant> implants, bool areSlotsKnownToBeEmpty = false)
        {
            foreach (var implant in implants)
            {
                SetImplant(implant, areSlotsKnownToBeEmpty);
            }
        }

        public bool CanEquipImplant(Implant implant, bool isSlotKnownToBeEmpty = false)
        {
            var currentImplant = isSlotKnownToBeEmpty ? null : UnequipImplant(implant.ImplantSlot);

            bool canEquipImplant = TreatmentValue >= implant.RequiredTreatmentValue
                && GetAbilityValue(implant.RequiredAbility) >= implant.RequiredAbilityValue;

            if (currentImplant != null)
            {
                SetImplant(currentImplant, isSlotKnownToBeEmpty: true);
            }

            return canEquipImplant;
        }

        public bool TryEquipImplant(Implant implant, bool isSlotKnownToBeEmpty = false)
        {
            var currentImplant = isSlotKnownToBeEmpty ? null : UnequipImplant(implant.ImplantSlot);

            bool canEquipImplant = TreatmentValue >= implant.RequiredTreatmentValue
                && GetAbilityValue(implant.RequiredAbility) >= implant.RequiredAbilityValue;

            if (canEquipImplant)
            {
                SetImplant(implant, isSlotKnownToBeEmpty: true);
            }
            else if (currentImplant != null)
            {
                SetImplant(currentImplant, isSlotKnownToBeEmpty: true);
            }

            return canEquipImplant;
        }

        public Implant GetMaxImplant(ImplantTemplate implantTemplate, bool isSlotKnownToBeEmpty = false)
        {
            var currentImplant = isSlotKnownToBeEmpty ? null : UnequipImplant(implantTemplate.ImplantSlot);

            var maxImplant = new Implant(implantTemplate, GetMaxImplantQL(implantTemplate));

            if (currentImplant != null)
            {
                SetImplant(currentImplant, isSlotKnownToBeEmpty: true);
            }

            return maxImplant;
        }

        public Implant EquipMaxImplant(ImplantTemplate implantTemplate, bool isSlotKnownToBeEmpty = false)
        {
            if (!isSlotKnownToBeEmpty)
            {
                UnequipImplant(implantTemplate.ImplantSlot);
            }

            var maxImplant = new Implant(implantTemplate, GetMaxImplantQL(implantTemplate));
            SetImplant(maxImplant, isSlotKnownToBeEmpty: true);

            return maxImplant;
        }

        private int GetMaxImplantQL(ImplantTemplate implantTemplate)
        {
            int ql = Implant.GetMaxImplantQL(GetAbilityValue(implantTemplate.RequiredAbility), TreatmentValue);
            return ShopBuyableQLMode ? Implant.ClampToShopBuyableQL(ql) : ql;
        }

        public bool IsImplantSlotFull(ImplantSlot implantSlot)
            => _implantConfiguration.IsImplantSlotFull(implantSlot);

        public bool IsImplantSlotEmpty(ImplantSlot implantSlot)
            => _implantConfiguration.IsImplantSlotEmpty(implantSlot);

        public int GetTotalImplantQL(IEnumerable<ImplantSlot> implantSlots = null)
            => _implantConfiguration.GetTotalImplantQL(implantSlots);

        public double GetAverageImplantQL(IEnumerable<ImplantSlot> implantSlots = null)
            => _implantConfiguration.GetAverageImplantQL(implantSlots);
    }
}
