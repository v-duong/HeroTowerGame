using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroData : ActorData
{
    public string Name { get; set; }

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

    protected Dictionary<BonusType, HeroStatBonus> statBonuses;
    /*
    private Equipment headgear;
    private Equipment bodyArmor;
    private Equipment gloves;
    private Equipment boots;
    private Equipment belt;
    private Equipment ring1;
    private Equipment ring2;
    private Equipment necklace;
    private Equipment mainHand;
    private Equipment offHand;
    private Archetype archetype;
    private Archetype subArchetype;
    */
    private Equipment[] equipList;
    private Archetype[] archetypeList;
    private List<AbilitySlot> abilityList;

    public HeroData()
    {
        Initialize();
    }

    public void Initialize()
    {
        Id = 0;
        Level = 1;
        Experience = 0;
        BaseHealth = 100;
        BaseSoulPoints = 50;
        BaseShield = 0;
        BaseStrength = 10;
        BaseAgility = 10;
        BaseIntelligence = 10;
        BaseWill = 10;
        BaseArmor = 0;
        BaseDodgeRating = 0;
        BaseResolveRating = 0;
        BaseAttackPhasing = 0;
        BaseMagicPhasing = 0;
        Resistances = new ElementResistances();
        equipList = new Equipment[10];
        archetypeList = new Archetype[2];
        statBonuses = new Dictionary<BonusType, HeroStatBonus>();
        abilityList = new List<AbilitySlot>();
        UpdateHeroAllStats();
    } 


    public void LevelUp()
    {
        ArchetypeBase a = archetypeList[0].Base;
        BaseHealth += a.healthGrowth;
        BaseSoulPoints += a.soulPointGrowth;
        BaseStrength += a.strengthGrowth;
        BaseIntelligence += a.intelligenceGrowth;
        BaseAgility += a.agilityGrowth;
        BaseWill += a.willGrowth;

        UpdateHeroAttributes(true);
        ApplyHealthBonuses();
        ApplySoulPointBonuses();
    }



    public bool EquipAbility(AbilityBase ability, int slot, IAbilitySource source)
    {
        if (slot >= 3)
            return false;
        abilityList[slot].SetAbilityToSlot(ability.idName, source);
        return true;
    }

    public bool RemoveAbility(int slot)
    {
        if (slot >= 3)
            return false;
        abilityList[slot] = null;
        return true;
    }

    public bool EquipToSlot(Equipment equip, EquipSlotType slot)
    {
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
                if (b.bonusType < (BonusType)0x600) //ignore local mods
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
                if (b.bonusType < (BonusType)0x600) //ignore local mods
                    RemoveStatBonus(affix.GetAffixValue(b.bonusType), b.bonusType, b.modifyType);
            }
        }
    }

    public void AddStatBonus(int value, BonusType type, ModifyType modifier)
    {
        if (!statBonuses.ContainsKey(type))
            statBonuses.Add(type, new HeroStatBonus());
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

    public void UpdateHeroAllStats()
    {
        UpdateHeroAttributes();
        ApplyHealthBonuses();
        ApplySoulPointBonuses();
        ApplyMagicPhasingBonuses();
        ApplyAttackPhasingBonuses();
    }

    public void UpdateHeroDefenses()
    {
        ApplyHealthBonuses();
        ApplyShieldBonuses();
        ApplySoulPointBonuses();
        ApplyDodgeRatingBonuses();
        ApplyResolveBonuses();
        ApplyArmorBonuses();
        ApplyMagicPhasingBonuses();
        ApplyAttackPhasingBonuses();
    }

    public void UpdateHeroAttributes(bool force = false)
    {
        if (statBonuses.ContainsKey(BonusType.STRENGTH) && statBonuses[BonusType.STRENGTH].isStatOutdated)
        {
            Strength = HeroStatCalculation((int)Math.Round(BaseStrength, MidpointRounding.AwayFromZero), statBonuses[BonusType.STRENGTH]);
        }
        else if (!statBonuses.ContainsKey(BonusType.STRENGTH))
            Strength = (int)Math.Round(BaseStrength, MidpointRounding.AwayFromZero);
        ApplyStrengthBonuses();

        if (statBonuses.ContainsKey(BonusType.INTELLIGENCE) && statBonuses[BonusType.INTELLIGENCE].isStatOutdated)
        {
            Intelligence = HeroStatCalculation((int)Math.Round(BaseIntelligence, MidpointRounding.AwayFromZero), statBonuses[BonusType.INTELLIGENCE]);
        } else if (!statBonuses.ContainsKey(BonusType.INTELLIGENCE))
            Intelligence = (int)Math.Round(BaseIntelligence, MidpointRounding.AwayFromZero);
        ApplyIntelligenceBonuses();

        if (statBonuses.ContainsKey(BonusType.AGILITY) && statBonuses[BonusType.AGILITY].isStatOutdated)
        {
            Agility = HeroStatCalculation((int)Math.Round(BaseAgility, MidpointRounding.AwayFromZero), statBonuses[BonusType.AGILITY]);
        }
        else if (!statBonuses.ContainsKey(BonusType.AGILITY))
            Agility = (int)Math.Round(BaseAgility, MidpointRounding.AwayFromZero);
        ApplyAgilityBonuses();

        if (statBonuses.ContainsKey(BonusType.WILL) && statBonuses[BonusType.WILL].isStatOutdated)
        {
            Will = HeroStatCalculation((int)Math.Round(BaseWill, MidpointRounding.AwayFromZero), statBonuses[BonusType.WILL]);
        }
        else if (!statBonuses.ContainsKey(BonusType.WILL))
            Will = (int)Math.Round(BaseWill, MidpointRounding.AwayFromZero);
        ApplyWillBonuses();
    }

    public void ApplyStrengthBonuses()
    {
        int armorMod = (int)Math.Round((double)Strength / 5, MidpointRounding.AwayFromZero);
        int attackDamageMod = (int)Math.Round((double)Strength / 10, MidpointRounding.AwayFromZero);

        if (!statBonuses.ContainsKey(BonusType.GLOBAL_ARMOR))
            statBonuses.Add(BonusType.GLOBAL_ARMOR, new HeroStatBonus());
        statBonuses[BonusType.GLOBAL_ARMOR].SetAdditiveAttributes(armorMod);

        if (!statBonuses.ContainsKey(BonusType.ATTACK_DAMAGE))
            statBonuses.Add(BonusType.ATTACK_DAMAGE, new HeroStatBonus());
        statBonuses[BonusType.ATTACK_DAMAGE].SetAdditiveAttributes(attackDamageMod);

        ApplyArmorBonuses();
    }

    public void ApplyIntelligenceBonuses()
    {
        int shieldMod = (int)Math.Round((double)Intelligence / 5, MidpointRounding.AwayFromZero);
        int spellDamageMod = (int)Math.Round((double)Intelligence / 10, MidpointRounding.AwayFromZero);

        if (!statBonuses.ContainsKey(BonusType.GLOBAL_MAX_SHIELD))
            statBonuses.Add(BonusType.GLOBAL_MAX_SHIELD, new HeroStatBonus());
        statBonuses[BonusType.GLOBAL_MAX_SHIELD].SetAdditiveAttributes(shieldMod);

        if (!statBonuses.ContainsKey(BonusType.SPELL_DAMAGE))
            statBonuses.Add(BonusType.SPELL_DAMAGE, new HeroStatBonus());
        statBonuses[BonusType.SPELL_DAMAGE].SetAdditiveAttributes(spellDamageMod);

        ApplyShieldBonuses();
    }

    public void ApplyAgilityBonuses()
    {
        int dodgeRatingMod = (int)Math.Round((double)Agility / 5, MidpointRounding.AwayFromZero);
        int attackSpeedMod = (int)Math.Round((double)Agility / 25, MidpointRounding.AwayFromZero);
        int castSpeedMod = (int)Math.Round((double)Agility / 25, MidpointRounding.AwayFromZero);

        if (!statBonuses.ContainsKey(BonusType.GLOBAL_DODGE_RATING))
            statBonuses.Add(BonusType.GLOBAL_DODGE_RATING, new HeroStatBonus());
        statBonuses[BonusType.GLOBAL_DODGE_RATING].SetAdditiveAttributes(dodgeRatingMod);

        if (!statBonuses.ContainsKey(BonusType.GLOBAL_ATTACK_SPEED))
            statBonuses.Add(BonusType.GLOBAL_ATTACK_SPEED, new HeroStatBonus());
        statBonuses[BonusType.GLOBAL_ATTACK_SPEED].SetAdditiveAttributes(attackSpeedMod);

        if (!statBonuses.ContainsKey(BonusType.CAST_SPEED))
            statBonuses.Add(BonusType.CAST_SPEED, new HeroStatBonus());
        statBonuses[BonusType.CAST_SPEED].SetAdditiveAttributes(castSpeedMod);

        ApplyDodgeRatingBonuses();
    }

    public void ApplyWillBonuses()
    {
        int resolveRatingMod = (int)Math.Round((double)Will / 5, MidpointRounding.AwayFromZero);
        int auraEffectMod = (int)Math.Round((double)Will / 20, MidpointRounding.AwayFromZero);

        if (!statBonuses.ContainsKey(BonusType.GLOBAL_RESOLVE_RATING))
            statBonuses.Add(BonusType.GLOBAL_RESOLVE_RATING, new HeroStatBonus());
        statBonuses[BonusType.GLOBAL_RESOLVE_RATING].SetAdditiveAttributes(resolveRatingMod);

        if (!statBonuses.ContainsKey(BonusType.AURA_EFFECT))
            statBonuses.Add(BonusType.AURA_EFFECT, new HeroStatBonus());
        statBonuses[BonusType.AURA_EFFECT].SetFlatAttributes(auraEffectMod);

        ApplyResolveBonuses();
    }

    public void ApplyHealthBonuses()
    {
        double percentage = CurrentHealth / MaximumHealth;
        if (statBonuses.ContainsKey(BonusType.MAX_HEALTH))
            MaximumHealth = HeroStatCalculation((int)Math.Round(BaseHealth, MidpointRounding.AwayFromZero), statBonuses[BonusType.MAX_HEALTH]);
        else
            MaximumHealth = (int)Math.Round(BaseHealth, MidpointRounding.AwayFromZero);
        CurrentHealth = (float)(MaximumHealth * percentage);
    }

    public void ApplySoulPointBonuses()
    {
        double percentage = CurrentSoulPoints / MaximumSoulPoints;
        if (statBonuses.ContainsKey(BonusType.MAX_SOULPOINTS))
            MaximumSoulPoints = HeroStatCalculation((int)Math.Round(BaseSoulPoints, MidpointRounding.AwayFromZero), statBonuses[BonusType.MAX_SOULPOINTS]);
        else
            MaximumSoulPoints = (int)Math.Round(BaseSoulPoints, MidpointRounding.AwayFromZero);
        CurrentSoulPoints = (float)(MaximumSoulPoints * percentage);
    }

    public void ApplyShieldBonuses()
    {
        int StatFromEquip = 0;
        foreach (Equipment e in equipList)
        {
            if (e != null && e.GetItemType() == ItemType.ARMOR)
            {
                StatFromEquip += ((Armor)e).shield;
            }
        }
        double percentage = CurrentShield / MaximumShield;
        if (statBonuses.ContainsKey(BonusType.GLOBAL_MAX_SHIELD))
            MaximumShield = HeroStatCalculation(BaseShield + StatFromEquip, statBonuses[BonusType.GLOBAL_MAX_SHIELD]);
        else
            MaximumShield = BaseShield + StatFromEquip;
        CurrentShield = (float)(MaximumShield * percentage);
    }

    public void ApplyArmorBonuses()
    {
        int StatFromEquip = 0;
        foreach (Equipment e in equipList)
        {
            if (e != null && e.GetItemType() == ItemType.ARMOR)
            {
                StatFromEquip += ((Armor)e).armor;
            }
        }
        if (statBonuses.ContainsKey(BonusType.GLOBAL_ARMOR))
            Armor = HeroStatCalculation(BaseArmor + StatFromEquip, statBonuses[BonusType.GLOBAL_ARMOR]);
        else
            Armor = BaseArmor + StatFromEquip;
    }

    public void ApplyDodgeRatingBonuses()
    {
        int StatFromEquip = 0;
        foreach (Equipment e in equipList)
        {
            if (e != null && e.GetItemType() == ItemType.ARMOR)
            {
                StatFromEquip += ((Armor)e).dodgeRating;
            }
        }
        if (statBonuses.ContainsKey(BonusType.GLOBAL_DODGE_RATING))
            DodgeRating = HeroStatCalculation(BaseDodgeRating + StatFromEquip, statBonuses[BonusType.GLOBAL_DODGE_RATING]);
        else
            DodgeRating = BaseDodgeRating + StatFromEquip;
    }

    public void ApplyResolveBonuses()
    {
        int StatFromEquip = 0;
        foreach (Equipment e in equipList)
        {
            if (e != null && e.GetItemType() == ItemType.ARMOR)
            {
                StatFromEquip += ((Armor)e).resolveRating;
            }
        }
        if (statBonuses.ContainsKey(BonusType.GLOBAL_RESOLVE_RATING))
            ResolveRating = HeroStatCalculation(BaseResolveRating + StatFromEquip, statBonuses[BonusType.GLOBAL_RESOLVE_RATING]);
        else
            ResolveRating = BaseResolveRating + StatFromEquip;
    }

    public void ApplyAttackPhasingBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.ATTACK_PHASING))
            AttackPhasing = HeroStatCalculation(BaseAttackPhasing, statBonuses[BonusType.ATTACK_PHASING]);
        else
            AttackPhasing = BaseAttackPhasing;
    }

    public void ApplyMagicPhasingBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.MAGIC_PHASING))
            MagicPhasing = HeroStatCalculation(BaseAttackPhasing, statBonuses[BonusType.MAGIC_PHASING]);
        else
            MagicPhasing = BaseMagicPhasing;
    }

    public static int HeroStatCalculation(int stat, HeroStatBonus bonuses)
    {
        if (bonuses.hasSetModifier)
        {
            bonuses.isStatOutdated = false;
            return bonuses.setModifier;
        }
        bonuses.isStatOutdated = false;
        return (int)Math.Round((stat + bonuses.FlatModifier + bonuses.FlatModifierFromAttributes) * (1 + (double)(bonuses.AdditiveModifier + bonuses.AdditiveModifierFromAttributes) / 100) * bonuses.CurrentMultiplier);
    }

}


public class AbilitySlot
{
    public string AbilityId { get; private set; }
    public IAbilitySource Source { get; private set; }

    public void SetAbilityToSlot(string id , IAbilitySource s)
    {
        AbilityId = id;
        Source = s;
    }
}