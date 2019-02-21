﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HeroActor : Actor {
    public static readonly int startingHealth = 100;
    public int BaseHealth { get; private set; }

    public static readonly int startingSoulPoints = 50;
    public int BaseSoulPoints { get; private set; }

    public int MaximumSoulPoints { get; private set; }
    public float CurrentSoulPoints { get; private set; }

    public int BaseStrength { get; private set; }
    public int BaseIntelligence { get; private set; }
    public int BaseAgility { get; private set; }
    public int BaseWill { get; private set; }
    

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

    // Use this for initialization
    public override void Start () {
        ActorAbility a = new ActorAbility
        {
            abilityBase = ResourceManager.Instance.GetAbilityBase(0)
        };
        a.InitializeActorAbility();
        AddAbilityToList(a);
        ActorAbility b = new ActorAbility
        {
            abilityBase = ResourceManager.Instance.GetAbilityBase(1)
        };
        b.InitializeActorAbility();
        AddAbilityToList(b);
    }
	
	// Update is called once per frame
	public void Update () {
        foreach(var x in abilitiesList)
        {
            x.StartFiring(this);
        }
	}

    public override void Death()
    {
        return;
    }

    public void AddAbilityToList(ActorAbility ability)
    {
        abilitiesList.Add(ability);
        var collider = this.transform.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.abilityBase.baseTargetRange;
        ability.collider = collider;
        collider.isTrigger = true;
    }

    public bool EquipToSlot(Equipment equip, EquipSlotType slot)
    {
        if (equip.Base.equipSlot != EquipSlotType.RING)
        {
            if (equip.Base.equipSlot != slot)
                return false;
        } else
        {
            if (slot != EquipSlotType.RING_SLOT_1 && slot != EquipSlotType.RING_SLOT_2)
                return false;
        }

        switch(slot)
        {
            case EquipSlotType.HEADGEAR:
                headgear = equip;
                break;
            case EquipSlotType.BODY_ARMOR:
                bodyArmor = equip;
                break;
            case EquipSlotType.BOOTS:
                boots = equip;
                break;
            case EquipSlotType.GLOVES:
                gloves = equip;
                break;
            case EquipSlotType.BELT:
                belt = equip;
                break;
            case EquipSlotType.NECKLACE:
                necklace = equip;
                break;
            case EquipSlotType.WEAPON:
                mainHand = equip;
                break;
            case EquipSlotType.OFF_HAND:
                offHand = equip;
                break;
            case EquipSlotType.RING_SLOT_1:
                ring1 = equip;
                break;
            case EquipSlotType.RING_SLOT_2:
                ring2 = equip;
                break;
            default:
                return false;
        }
        equip.equippedHeroId = Id;
        ApplyEquipmentBonuses(equip.prefixes);
        ApplyEquipmentBonuses(equip.suffixes);
        ApplyEquipmentBonuses(equip.innate);
        return true;
    }

    private void ApplyEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach(AffixBonusBase b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x600)
                    AddStatBonus(affix.GetAffixValue(b.bonusType), b.bonusType, b.modifyType);
            }
        }
    }

    private void RemoveEquipemntBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusBase b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x600)
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
        UpdateHeroDefenses();
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

    public void UpdateHeroAttributes()
    {
        if (statBonuses.ContainsKey(BonusType.STRENGTH) && statBonuses[BonusType.STRENGTH].isStatOutdated)
        {
            Strength = HeroStatCalculation(BaseStrength, statBonuses[BonusType.STRENGTH]);
            ApplyStrengthBonuses();
        }
        if (statBonuses.ContainsKey(BonusType.INTELLIGENCE) && statBonuses[BonusType.INTELLIGENCE].isStatOutdated)
        {
            Intelligence = HeroStatCalculation(BaseIntelligence, statBonuses[BonusType.INTELLIGENCE]);
            ApplyIntelligenceBonuses();
        }
        if (statBonuses.ContainsKey(BonusType.AGILITY) && statBonuses[BonusType.AGILITY].isStatOutdated)
        {
            Agility = HeroStatCalculation(BaseAgility, statBonuses[BonusType.AGILITY]);
            ApplyAgilityBonuses();
        }
        if (statBonuses.ContainsKey(BonusType.WILL) && statBonuses[BonusType.WILL].isStatOutdated)
        {
            Will = HeroStatCalculation(BaseWill, statBonuses[BonusType.WILL]);
            ApplyWillBonuses();
        }
    }

    public void ApplyStrengthBonuses()
    {
        int armorMod = (int)Math.Floor( (float)Strength / 5 );
        int attackDamageMod = (int)Math.Floor((float)Strength / 10);

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
        int shieldMod = (int)Math.Floor( (float)Intelligence / 5 );
        int spellDamageMod = (int)Math.Floor((float)Intelligence / 10);

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
        int dodgeRatingMod = (int)Math.Floor((float)Agility / 5);
        int attackSpeedMod = (int)Math.Floor((float)Agility / 25);
        int castSpeedMod = (int)Math.Floor((float)Agility / 25);

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
        int resolveRatingMod = (int)Math.Floor((float)Will / 5);
        int soulPointsMod = (int)Math.Floor((float)Will / 10);
        int healthMod = (int)Math.Floor((float)Will / 2);

        if (!statBonuses.ContainsKey(BonusType.GLOBAL_RESOLVE_RATING))
            statBonuses.Add(BonusType.GLOBAL_RESOLVE_RATING, new HeroStatBonus());
        statBonuses[BonusType.GLOBAL_RESOLVE_RATING].SetAdditiveAttributes(resolveRatingMod);

        if (!statBonuses.ContainsKey(BonusType.MAX_SOULPOINTS))
            statBonuses.Add(BonusType.MAX_SOULPOINTS, new HeroStatBonus());
        statBonuses[BonusType.MAX_SOULPOINTS].SetFlatAttributes(soulPointsMod);

        if (!statBonuses.ContainsKey(BonusType.MAX_HEALTH))
            statBonuses.Add(BonusType.MAX_HEALTH, new HeroStatBonus());
        statBonuses[BonusType.MAX_HEALTH].SetFlatAttributes(healthMod);

        ApplyHealthBonuses();
        ApplySoulPointBonuses();
        ApplyResolveBonuses();
    }

    public void ApplyHealthBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.MAX_HEALTH))
            MaximumHealth = HeroStatCalculation(startingHealth, statBonuses[BonusType.MAX_HEALTH]);
    }

    public void ApplySoulPointBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.MAX_SOULPOINTS))
            MaximumSoulPoints = HeroStatCalculation(startingSoulPoints, statBonuses[BonusType.MAX_SOULPOINTS]);
    }

    public void ApplyShieldBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_MAX_SHIELD))
            MaximumShield = HeroStatCalculation(BaseShield, statBonuses[BonusType.GLOBAL_MAX_SHIELD]);
    }

    public void ApplyArmorBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_ARMOR))
            Armor = HeroStatCalculation(BaseArmor, statBonuses[BonusType.GLOBAL_ARMOR]);
    }

    public void ApplyDodgeRatingBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_DODGE_RATING))
            DodgeRating = HeroStatCalculation(BaseDodgeRating, statBonuses[BonusType.GLOBAL_DODGE_RATING]);
    }

    public void ApplyResolveBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_RESOLVE_RATING))
            ResolveRating = HeroStatCalculation(BaseResolveRating, statBonuses[BonusType.GLOBAL_RESOLVE_RATING]);
    }

    public void ApplyAttackPhasingBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.ATTACK_PHASING))
            AttackPhasing = HeroStatCalculation(BaseAttackPhasing, statBonuses[BonusType.ATTACK_PHASING]);
    }

    public void ApplyMagicPhasingBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.MAGIC_PHASING))
            MagicPhasing = HeroStatCalculation(BaseAttackPhasing, statBonuses[BonusType.MAGIC_PHASING]);
    }

    public static int HeroStatCalculation(int stat, HeroStatBonus bonuses)
    {
        if (bonuses.hasSetModifier)
        {
            bonuses.isStatOutdated = false;
            return bonuses.setModifier;
        }
        bonuses.isStatOutdated = false;
        return (int)Math.Round((stat + bonuses.FlatModifier + bonuses.FlatModifierFromAttributes) * (1 + (float)(bonuses.AdditiveModifier + bonuses.AdditiveModifierFromAttributes)/100) * bonuses.CurrentMultiplier);
    }
}


public class HeroStatBonus
{
    public int FlatModifier { get; private set; }
    public int FlatModifierFromAttributes { get; private set; }
    public int AdditiveModifier { get; private set; }
    public int AdditiveModifierFromAttributes { get; private set; }
    public List<int> MultiplyModifiers { get; private set; }
    public float CurrentMultiplier { get; private set; }
    public bool hasSetModifier;
    public int setModifier;
    public bool isStatOutdated;

    public HeroStatBonus()
    {
        FlatModifier = 0;
        FlatModifierFromAttributes = 0;
        AdditiveModifier = 0;
        AdditiveModifierFromAttributes = 0;
        MultiplyModifiers = new List<int>();
        CurrentMultiplier = 1.00f;
        hasSetModifier = false;
        setModifier = 0;
        isStatOutdated = true;
    }

    public void AddToFlat(int value)
    {
        FlatModifier += value;
        isStatOutdated = true;
    }

    public void SetFlatAttributes(int value)
    {
        FlatModifierFromAttributes = value;
        isStatOutdated = true;
    }

    public void AddToAdditive(int value)
    {
        AdditiveModifier += value;
        isStatOutdated = true;
    }

    public void SetAdditiveAttributes(int value)
    {
        AdditiveModifierFromAttributes = value;
        isStatOutdated = true;
    }

    public void AddToMultiply(int value)
    {
        MultiplyModifiers.Add(value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    public void RemoveFromMultiply(int value)
    {
        MultiplyModifiers.Remove(value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    public void UpdateCurrentMultiply()
    {
        CurrentMultiplier = 1.0f;
        foreach (int i in MultiplyModifiers)
            CurrentMultiplier *= i;
    }
}