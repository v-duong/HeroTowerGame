using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroActor : Actor
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

    public HeroActor()
    {
        Id = 0;
        Level = 1;
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
        abilitiesList = new List<ActorAbility>();
        equipList = new Equipment[10];
        archetypeList = new Archetype[2];
        statBonuses = new Dictionary<BonusType, HeroStatBonus>();
    }

    // Use this for initialization
    public override void Start()
    {
        ActorAbility a = new ActorAbility
        {
            //abilityBase = ResourceManager.Instance.GetAbilityBase(0)
        };
        a.InitializeActorAbility();
        AddAbilityToList(a);
        ActorAbility b = new ActorAbility
        {
            //abilityBase = ResourceManager.Instance.GetAbilityBase(1)
        };
        b.InitializeActorAbility();
        AddAbilityToList(b);
    }

    // Update is called once per frame
    public void Update()
    {
        foreach (var x in abilitiesList)
        {
            x.StartFiring(this);
        }
    }

    public override void Death()
    {
        return;
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

        UpdateHeroAttributes();
        ApplyHealthBonuses();
        ApplySoulPointBonuses();
    }

    public void AddAbilityToList(ActorAbility ability)
    {
        abilitiesList.Add(ability);
        var collider = this.transform.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.abilityBase.targetRange;
        ability.collider = collider;
        collider.isTrigger = true;
    }

    public bool EquipToSlot(Equipment equip, EquipSlotType slot)
    {
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
        return true;
    }

    private void ApplyEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x600)
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
            Strength = HeroStatCalculation((int)Math.Round(BaseStrength, MidpointRounding.AwayFromZero), statBonuses[BonusType.STRENGTH]);
        }
        ApplyStrengthBonuses();

        if (statBonuses.ContainsKey(BonusType.INTELLIGENCE) && statBonuses[BonusType.INTELLIGENCE].isStatOutdated)
        {
            Intelligence = HeroStatCalculation((int)Math.Round(BaseIntelligence, MidpointRounding.AwayFromZero), statBonuses[BonusType.INTELLIGENCE]);
        }
        ApplyIntelligenceBonuses();

        if (statBonuses.ContainsKey(BonusType.AGILITY) && statBonuses[BonusType.AGILITY].isStatOutdated)
        {
            Agility = HeroStatCalculation((int)Math.Round(BaseAgility, MidpointRounding.AwayFromZero), statBonuses[BonusType.AGILITY]);
        }
        ApplyAgilityBonuses();

        if (statBonuses.ContainsKey(BonusType.WILL) && statBonuses[BonusType.WILL].isStatOutdated)
        {
            Will = HeroStatCalculation((int)Math.Round(BaseWill, MidpointRounding.AwayFromZero), statBonuses[BonusType.WILL]);
        }
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
        double percentage = CurrentShield / MaximumShield;
        if (statBonuses.ContainsKey(BonusType.GLOBAL_MAX_SHIELD))
            MaximumShield = HeroStatCalculation(BaseShield, statBonuses[BonusType.GLOBAL_MAX_SHIELD]);
        else
            MaximumShield = BaseShield;
        CurrentShield = (float)(MaximumShield * percentage);
    }

    public void ApplyArmorBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_ARMOR))
            Armor = HeroStatCalculation(BaseArmor, statBonuses[BonusType.GLOBAL_ARMOR]);
        else
            Armor = BaseArmor;
    }

    public void ApplyDodgeRatingBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_DODGE_RATING))
            DodgeRating = HeroStatCalculation(BaseDodgeRating, statBonuses[BonusType.GLOBAL_DODGE_RATING]);
        else
            DodgeRating = BaseDodgeRating;
    }

    public void ApplyResolveBonuses()
    {
        if (statBonuses.ContainsKey(BonusType.GLOBAL_RESOLVE_RATING))
            ResolveRating = HeroStatCalculation(BaseResolveRating, statBonuses[BonusType.GLOBAL_RESOLVE_RATING]);
        else
            ResolveRating = BaseResolveRating;
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
        double mult = 1.0d;
        foreach (int i in MultiplyModifiers)
            mult *= (1d + i / 100d);
        CurrentMultiplier = (float)mult;
    }
}