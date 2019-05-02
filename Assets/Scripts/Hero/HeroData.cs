using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroData : ActorData
{

    public float BaseStrength { get; private set; }
    public float BaseIntelligence { get; private set; }
    public float BaseAgility { get; private set; }
    public float BaseWill { get; private set; }

    public int Strength { get; private set; }
    public int Intelligence { get; private set; }
    public int Agility { get; private set; }
    public int Will { get; private set; }

    public int Armor { get; private set; }
    public int DodgeRating { get; private set; }
    public int ResolveRating { get; private set; }
    public int AttackPhasing { get; private set; }
    public int MagicPhasing { get; private set; }

    private Dictionary<BonusType, StatBonus> statBonuses;
    private Dictionary<BonusType, StatBonus> archetypeStatBonuses;
    private Dictionary<BonusType, StatBonus> attributeStatBonuses;

    private Equipment[] equipList;
    private HeroArchetypeData[] archetypeList;
    private List<AbilitySlot> abilitySlotList;

    public HeroArchetypeData PrimaryArchetype => archetypeList[0];
    public HeroArchetypeData SecondaryArchetype => archetypeList[1];

    private HeroData()
    {
        Initialize();
    }

    private HeroData(string name)
    {
        Initialize(name);
    }

    public void InitHeroActor(GameObject actor)
    {
        HeroActor hero = actor.AddComponent<HeroActor>();
        hero.Initialize(this);
    }

    private void Initialize(string name = "")
    {
        Id = 0;
        Name = name;
        Level = 0;
        Experience = 0;
        BaseHealth = 100;
        BaseSoulPoints = 50;
        BaseManaShield = 0;
        BaseStrength = 10;
        BaseAgility = 10;
        BaseIntelligence = 10;
        BaseWill = 10;
        BaseArmor = 0;
        BaseDodgeRating = 0;
        BaseResolveRating = 0;
        BaseAttackPhasing = 0;
        BaseMagicPhasing = 0;
        movementSpeed = 3f;
        Resistances = new ElementResistances();
        equipList = new Equipment[10];
        archetypeList = new HeroArchetypeData[2];
        statBonuses = new Dictionary<BonusType, StatBonus>();
        archetypeStatBonuses = new Dictionary<BonusType, StatBonus>();
        attributeStatBonuses = new Dictionary<BonusType, StatBonus>();
        abilitySlotList = new List<AbilitySlot>() { new AbilitySlot(0), new AbilitySlot(1) };
        UpdateHeroAllStats();
    }

    public static HeroData CreateNewHero(string name, ArchetypeItem primaryArchetype, ArchetypeItem subArchetype = null)
    {
        HeroData hero = new HeroData(name);
        hero.archetypeList[0] = new HeroArchetypeData(primaryArchetype, hero);
        hero.archetypeList[1] = new HeroArchetypeData(subArchetype, hero);
        hero.LevelUp();
        return hero;
    }

    public void LevelUp()
    {
        if (Level == 100)
            return;
        Level++;
        HeroArchetypeData primaryArchetype = archetypeList[0];
        BaseHealth += primaryArchetype.HealthGrowth;
        BaseSoulPoints += primaryArchetype.SoulPointGrowth;
        BaseStrength += primaryArchetype.StrengthGrowth;
        BaseIntelligence += primaryArchetype.IntelligenceGrowth;
        BaseAgility += primaryArchetype.AgilityGrowth;
        BaseWill += primaryArchetype.WillGrowth;

        UpdateHeroAllStats();
    }

    public bool EquipAbility(AbilityBase ability, int slot, IAbilitySource source)
    {
        if (slot >= 3)
            return false;
        if (slot == 0)
        {
            if (abilitySlotList[1].Ability != null
                && abilitySlotList[1].Ability.abilityBase == ability && abilitySlotList[1].source == source)
            {
                UnequipAbility(1);
            }
            abilitySlotList[slot].SetAbilityToSlot(ability, source);
        }
        else
        {
            if (abilitySlotList[0].Ability != null
                && abilitySlotList[0].Ability.abilityBase == ability && abilitySlotList[0].source == source)
            {
                UnequipAbility(0);
            }
            abilitySlotList[slot].SetAbilityToSlot(ability, source);
        }
        UpdateAbilities();
        return true;
    }

    public bool UnequipAbility(int slot)
    {
        if (slot >= 3)
            return false;
        abilitySlotList[slot].ClearAbility();
        return true;
    }

    public ActorAbility GetAbilityFromSlot(int slot)
    {
        return abilitySlotList[slot].Ability;
    }

    public int GetAbilitySlotLevel(int slot)
    {
        return abilitySlotList[slot].GetAbilityLevel();
    }

    public Equipment GetEquipmentInSlot(EquipSlotType slot)
    {
        return equipList[(int)slot];
    }

    public bool EquipToSlot(Equipment equip, EquipSlotType slot)
    {
        if (equip.IsEquipped)
            return false;
        if (equipList[(int)slot] != null)
            UnequipFromSlot(slot);

        if (equip.Base.equipSlot != EquipSlotType.RING)
        {
            if (equip.Base.equipSlot != slot)
                return false;
        }
        else
        {
            if (slot != EquipSlotType.RING_SLOT_1 && slot != EquipSlotType.RING_SLOT_2)
                return false;
        }

        switch (slot)
        {
            case EquipSlotType.HEADGEAR:
            case EquipSlotType.BODY_ARMOR:
            case EquipSlotType.BOOTS:
            case EquipSlotType.GLOVES:
            case EquipSlotType.BELT:
            case EquipSlotType.NECKLACE:
            case EquipSlotType.WEAPON:
            case EquipSlotType.OFF_HAND:
            case EquipSlotType.RING_SLOT_1:
            case EquipSlotType.RING_SLOT_2:
                equipList[(int)slot] = equip;
                break;

            default:
                return false;
        }
        equip.equippedToHero = this;
        ApplyEquipmentBonuses(equip.prefixes);
        ApplyEquipmentBonuses(equip.suffixes);
        ApplyEquipmentBonuses(equip.innate);
        UpdateHeroAllStats();

        return true;
    }

    public bool UnequipFromSlot(EquipSlotType slot)
    {
        if (equipList[(int)slot] == null)
            return false;
        Equipment equip = equipList[(int)slot];
        equip.equippedToHero = null;
        RemoveEquipmentBonuses(equip.prefixes);
        RemoveEquipmentBonuses(equip.suffixes);
        RemoveEquipmentBonuses(equip.innate);
        UpdateHeroAllStats();
        return true;
    }

    private void ApplyEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x700) //ignore local mods
                    AddStatBonus(affix.GetAffixValue(b.bonusType), b.bonusType, b.modifyType);
            }
        }
    }

    private void RemoveEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x700) //ignore local mods
                    RemoveStatBonus(affix.GetAffixValue(b.bonusType), b.bonusType, b.modifyType);
            }
        }
    }

    public void AddStatBonus(int value, BonusType type, ModifyType modifier)
    {
        if (!statBonuses.ContainsKey(type))
            statBonuses.Add(type, new StatBonus());
        switch (modifier)
        {
            case ModifyType.FLAT_ADDITION:
                statBonuses[type].AddToFlat(value);
                break;

            case ModifyType.ADDITIVE:
                statBonuses[type].AddToAdditive(value);
                break;

            case ModifyType.MULTIPLY:
                statBonuses[type].AddToMultiply(value);
                break;

            default:
                return;
        }
    }

    public void RemoveStatBonus(int value, BonusType type, ModifyType modifier)
    {
        switch (modifier)
        {
            case ModifyType.FLAT_ADDITION:
                statBonuses[type].AddToFlat(-value);
                break;

            case ModifyType.ADDITIVE:
                statBonuses[type].AddToAdditive(-value);
                break;

            case ModifyType.MULTIPLY:
                statBonuses[type].RemoveFromMultiply(value);
                break;

            default:
                return;
        }
    }

    public void AddArchetypeStatBonus(int value, BonusType type, ModifyType modifier)
    {
        if (!archetypeStatBonuses.ContainsKey(type))
            archetypeStatBonuses.Add(type, new StatBonus());
        switch (modifier)
        {
            case ModifyType.FLAT_ADDITION:
                archetypeStatBonuses[type].AddToFlat(value);
                break;

            case ModifyType.ADDITIVE:
                archetypeStatBonuses[type].AddToAdditive(value);
                break;

            case ModifyType.MULTIPLY:
                archetypeStatBonuses[type].AddToMultiply(value);
                break;

            default:
                return;
        }
    }

    public void RemoveArchetypeStatBonus(int value, BonusType type, ModifyType modifier)
    {
        if (!archetypeStatBonuses.ContainsKey(type))
            return;
        switch (modifier)
        {
            case ModifyType.FLAT_ADDITION:
                archetypeStatBonuses[type].AddToFlat(-value);
                break;

            case ModifyType.ADDITIVE:
                archetypeStatBonuses[type].AddToAdditive(-value);
                break;

            case ModifyType.MULTIPLY:
                archetypeStatBonuses[type].RemoveFromMultiply(value);
                break;

            default:
                return;
        }
    }

    public void UpdateHeroAllStats()
    {
        UpdateHeroAttributes();
        UpdateAbilities();
        UpdateHeroDefenses();
    }

    public void UpdateHeroDefenses()
    {
        ApplyHealthBonuses();
        ApplySoulPointBonuses();
        CalculateDefenses();
        AttackPhasing = CalculateHeroStat(BonusType.ATTACK_PHASING, BaseAttackPhasing);
        MagicPhasing = CalculateHeroStat(BonusType.MAGIC_PHASING, BaseMagicPhasing);
    }

    private void UpdateHeroAttributes()
    {
        Strength = (int)Math.Round(CalculateHeroStat(BonusType.STRENGTH, BaseStrength), MidpointRounding.AwayFromZero);
        ApplyStrengthBonuses();

        Intelligence = (int)Math.Round(CalculateHeroStat(BonusType.INTELLIGENCE, BaseIntelligence), MidpointRounding.AwayFromZero);
        ApplyIntelligenceBonuses();

        Agility = (int)Math.Round(CalculateHeroStat(BonusType.AGILITY, BaseAgility), MidpointRounding.AwayFromZero);
        ApplyAgilityBonuses();

        Will = (int)Math.Round(CalculateHeroStat(BonusType.WILL, BaseWill), MidpointRounding.AwayFromZero);
        ApplyWillBonuses();
    }

    private void ApplyStrengthBonuses()
    {
        /*
         * +1% Armor per 5 Str
         * +1% Attack Damage per 10 Str
         */
        int armorMod = (int)Math.Round(Strength / 5d, MidpointRounding.AwayFromZero);
        int attackDamageMod = (int)Math.Round(Strength / 10d, MidpointRounding.AwayFromZero);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_ARMOR))
            attributeStatBonuses.Add(BonusType.GLOBAL_ARMOR, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_ARMOR].SetAdditive(armorMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.ATTACK_DAMAGE))
            attributeStatBonuses.Add(BonusType.ATTACK_DAMAGE, new StatBonus());
        attributeStatBonuses[BonusType.ATTACK_DAMAGE].SetAdditive(attackDamageMod);
    }

    private void ApplyIntelligenceBonuses()
    {
        /*
         * +1% Shield per 5 Int
         * +1% Spell Damage per 10 Int
         */
        int shieldMod = (int)Math.Round(Intelligence / 5d, MidpointRounding.AwayFromZero);
        int spellDamageMod = (int)Math.Round(Intelligence / 10d, MidpointRounding.AwayFromZero);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_MAX_SHIELD))
            attributeStatBonuses.Add(BonusType.GLOBAL_MAX_SHIELD, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_MAX_SHIELD].SetAdditive(shieldMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.SPELL_DAMAGE))
            attributeStatBonuses.Add(BonusType.SPELL_DAMAGE, new StatBonus());
        attributeStatBonuses[BonusType.SPELL_DAMAGE].SetAdditive(spellDamageMod);
    }

    private void ApplyAgilityBonuses()
    {
        /*
         * +1% Dodge per 5 Agi
         * +1% Attack/Cast Speed per 25 Agi
         */
        int dodgeRatingMod = (int)Math.Round(Agility / 5d, MidpointRounding.AwayFromZero);
        int attackSpeedMod = (int)Math.Round(Agility / 25d, MidpointRounding.AwayFromZero);
        int castSpeedMod = (int)Math.Round(Agility / 25d, MidpointRounding.AwayFromZero);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_DODGE_RATING))
            attributeStatBonuses.Add(BonusType.GLOBAL_DODGE_RATING, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_DODGE_RATING].SetAdditive(dodgeRatingMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_ATTACK_SPEED))
            attributeStatBonuses.Add(BonusType.GLOBAL_ATTACK_SPEED, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_ATTACK_SPEED].SetAdditive(attackSpeedMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.CAST_SPEED))
            attributeStatBonuses.Add(BonusType.CAST_SPEED, new StatBonus());
        attributeStatBonuses[BonusType.CAST_SPEED].SetAdditive(castSpeedMod);
    }

    private void ApplyWillBonuses()
    {
        /*
         * +1% Resolve per 5 Will
         * +1% Debuff Damage per 10 Will
         */
        int resolveRatingMod = (int)Math.Round(Will / 5d, MidpointRounding.AwayFromZero);
        int debuffDamageMod = (int)Math.Round(Will / 10d, MidpointRounding.AwayFromZero);

        BonusType bonus1 = BonusType.GLOBAL_RESOLVE_RATING;
        BonusType bonus2 = BonusType.DEBUFF_DAMAGE;

        if (!attributeStatBonuses.ContainsKey(bonus1))
            attributeStatBonuses.Add(bonus1, new StatBonus());
        attributeStatBonuses[bonus1].SetAdditive(resolveRatingMod);

        if (!attributeStatBonuses.ContainsKey(bonus2))
            attributeStatBonuses.Add(bonus2, new StatBonus());
        attributeStatBonuses[bonus2].SetAdditive(debuffDamageMod);
    }

    private void ApplyHealthBonuses()
    {
        double percentage = CurrentHealth / MaximumHealth;
        MaximumHealth = (int)Math.Round(CalculateHeroStat(BonusType.MAX_HEALTH, BaseHealth), MidpointRounding.AwayFromZero);
        CurrentHealth = (float)(MaximumHealth * percentage);
    }

    private void ApplySoulPointBonuses()
    {
        double percentage = CurrentSoulPoints / MaximumSoulPoints;
        MaximumSoulPoints = (int)Math.Round(CalculateHeroStat(BonusType.MAX_SOULPOINTS, BaseSoulPoints), MidpointRounding.AwayFromZero);
        CurrentSoulPoints = (float)(MaximumSoulPoints * percentage);
    }

    public void CalculateDefenses()
    {
        int ArmorFromEquip = 0;
        int ShieldFromEquip = 0;
        int DodgeFromEquip = 0;
        int ResolveFromEquip = 0;
        foreach (Equipment e in equipList)
        {
            if (e != null && e.GetItemType() == EquipmentType.ARMOR)
            {
                Armor equip = e as Armor;
                ArmorFromEquip += equip.armor;
                ShieldFromEquip += equip.shield;
                DodgeFromEquip += equip.dodgeRating;
                ResolveFromEquip += equip.resolveRating;
            }
        }

        Armor = CalculateHeroStat(BonusType.GLOBAL_ARMOR, BaseArmor + ArmorFromEquip);
        MaximumManaShield = CalculateHeroStat(BonusType.GLOBAL_MAX_SHIELD, BaseManaShield + ShieldFromEquip);
        DodgeRating = CalculateHeroStat(BonusType.GLOBAL_DODGE_RATING, BaseDodgeRating + DodgeFromEquip);
        ResolveRating = CalculateHeroStat(BonusType.GLOBAL_RESOLVE_RATING, BaseResolveRating + ResolveFromEquip);
    }

    public int CalculateHeroStat(BonusType type, int stat)
    {
        return (int)Math.Round(CalculateHeroStat(type, (double)stat), MidpointRounding.AwayFromZero);
    }

    public double CalculateHeroStat(BonusType type, double stat)
    {
        return GetTotalStatBonus(type).CalculateStat(stat);
    }

    public StatBonus GetTotalStatBonus(BonusType type, StatBonus existingBonus = null)
    {
        StatBonus resultBonus;
        if (existingBonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = existingBonus;
        bool hasStatBonus = false, hasAttributeBonus = false, hasArchetypeBonus = false;

        if (statBonuses.TryGetValue(type, out StatBonus baseBonus))
            hasStatBonus = true;
        if (attributeStatBonuses.TryGetValue(type, out StatBonus attributeBonus))
            hasAttributeBonus = true;
        if (archetypeStatBonuses.TryGetValue(type, out StatBonus archetypeBonus))
            hasArchetypeBonus = true;

        if (!hasAttributeBonus && !hasStatBonus && !hasArchetypeBonus)
        {
            return resultBonus;
        }

        if (hasArchetypeBonus && archetypeBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = archetypeBonus.setModifier;
            return resultBonus;
        }
        else if (hasStatBonus && baseBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = baseBonus.setModifier;
        }

        if (hasStatBonus)
        {
            resultBonus.AddToFlat(baseBonus.FlatModifier);
            resultBonus.AddToAdditive(baseBonus.AdditiveModifier);
            resultBonus.AddToMultiply((baseBonus.CurrentMultiplier - 1) * 100);
        }
        if (hasArchetypeBonus)
        {
            resultBonus.AddToFlat(archetypeBonus.FlatModifier);
            resultBonus.AddToAdditive(archetypeBonus.AdditiveModifier);
            resultBonus.AddToMultiply((archetypeBonus.CurrentMultiplier - 1) * 100);
        }
        if (hasAttributeBonus)
        {
            resultBonus.AddToFlat(attributeBonus.FlatModifier);
            resultBonus.AddToAdditive(attributeBonus.AdditiveModifier);
            resultBonus.AddToMultiply((attributeBonus.CurrentMultiplier - 1) * 100);
        }
        return resultBonus;
    }

    public void UpdateAbilities()
    {
        foreach (AbilitySlot abilitySlot in abilitySlotList)
        {
            if (abilitySlot.Ability != null)
            {
                abilitySlot.UpdateAbilityLevel();
                abilitySlot.Ability.UpdateAbilityStats(this);
            }
        }
    }

    private class AbilitySlot
    {
        public ActorAbility Ability { get; private set; }
        public IAbilitySource source;
        private AbilitySourceType sourceType;
        public readonly int slot;

        public AbilitySlot(int slot)
        {
            this.slot = slot;
        }

        public void SetAbilityToSlot(AbilityBase abilityBase, IAbilitySource source)
        {
            Ability = new ActorAbility(abilityBase);
            this.source = source;
            if (source.GetType() == typeof(HeroArchetypeData))
                sourceType = AbilitySourceType.ARCHETYPE;
        }

        public int GetAbilityLevel()
        {
            return source.GetAbilityLevel();
        }

        public void UpdateAbilityLevel()
        {
            Ability.UpdateAbilityLevel(GetAbilityLevel());
        }

        public void ClearAbility()
        {
            Ability = null;
            source = null;
        }
    }
}