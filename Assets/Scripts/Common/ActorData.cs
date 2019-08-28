using System;
using System.Collections.Generic;

public abstract class ActorData
{
    public Guid Id { get; protected set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string Name { get; set; }
    public Actor CurrentActor { get; protected set; }

    public float BaseHealth { get; protected set; }
    public int MaximumHealth { get; protected set; }
    public float CurrentHealth { get; set; }
    public int MinimumHealth { get; protected set; } //for cases of invincible/phased actors
    public bool HealthIsHitsToKill { get; protected set; } //health is number of hits to kill
    public float HealthRegenRate { get; protected set; }

    public float BaseSoulPoints { get; protected set; }
    public int MaximumSoulPoints { get; set; }
    public float CurrentSoulPoints { get; set; }

    public int BaseManaShield { get; protected set; }
    public int MaximumManaShield { get; protected set; }
    public float CurrentManaShield { get; set; }
    public float ShieldRegenRate { get; protected set; }
    public float ShieldRestoreRate { get; protected set; }
    public float ShieldRestoreDelayModifier { get; protected set; }
    public float CurrentShieldDelay { get; set; }

    public int BaseArmor { get; protected set; }
    public int BaseDodgeRating { get; protected set; }
    public int BaseAttackPhasing { get; protected set; }
    public int BaseMagicPhasing { get; protected set; }
    public int BaseResolveRating { get; protected set; }

    public int Armor { get; protected set; }
    public int DodgeRating { get; protected set; }
    public int ResolveRating { get; protected set; }
    public int AttackPhasing { get; protected set; }
    public int MagicPhasing { get; protected set; }

    public int PhysicalNegation { get => ElementData.GetNegation(ElementType.PHYSICAL); set => ElementData.SetNegation(ElementType.PHYSICAL, value); }
    public int FireNegation { get => ElementData.GetNegation(ElementType.FIRE); set => ElementData.SetNegation(ElementType.FIRE, value); }
    public int ColdNegation { get => ElementData.GetNegation(ElementType.COLD); set => ElementData.SetNegation(ElementType.COLD, value); }
    public int LightningNegation { get => ElementData.GetNegation(ElementType.LIGHTNING); set => ElementData.SetNegation(ElementType.LIGHTNING, value); }
    public int EarthNegation { get => ElementData.GetNegation(ElementType.EARTH); set => ElementData.SetNegation(ElementType.EARTH, value); }
    public int DivineNegation { get => ElementData.GetNegation(ElementType.DIVINE); set => ElementData.SetNegation(ElementType.DIVINE, value); }
    public int VoidNegation { get => ElementData.GetNegation(ElementType.VOID); set => ElementData.SetNegation(ElementType.VOID, value); }

    protected Dictionary<BonusType, StatBonusCollection> statBonuses;
    protected Dictionary<BonusType, StatBonus> temporaryBonuses;

    protected ElementalData ElementData { get; private set; }

    public HashSet<GroupType> groupTypes { get; protected set; }

    public float movementSpeed;

    protected ActorData()
    {
        Id = Guid.NewGuid();
        ElementData = new ElementalData();
        statBonuses = new Dictionary<BonusType, StatBonusCollection>();
        temporaryBonuses = new Dictionary<BonusType, StatBonus>();
        groupTypes = new HashSet<GroupType>();
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }

    public bool IsAlive
    {
        get { return GetCurrentHealth() > 0.0f; }
    }

    public bool IsDead
    {
        get { return GetCurrentHealth() <= 0.0f; }
    }

    public void AddStatBonus(BonusType type, GroupType restriction, ModifyType modifier, float value)
    {
        if (!statBonuses.ContainsKey(type))
            statBonuses.Add(type, new StatBonusCollection());
        statBonuses[type].AddBonus(restriction, modifier, value);
    }

    public void RemoveStatBonus(BonusType type, GroupType restriction, ModifyType modifier, float value)
    {
        statBonuses[type].RemoveBonus(restriction, modifier, value);
    }

    public void AddTemporaryBonus(float value, BonusType type, ModifyType modifier)
    {
        if (!temporaryBonuses.ContainsKey(type))
            temporaryBonuses.Add(type, new StatBonus());
        temporaryBonuses[type].AddBonus(modifier, value);
        UpdateActorData();
    }

    public void RemoveTemporaryBonus(float value, BonusType type, ModifyType modifier)
    {
        temporaryBonuses[type].RemoveBonus(modifier, value);
        UpdateActorData();
    }

    public void ClearTemporaryBonuses()
    {
        temporaryBonuses.Clear();
        UpdateActorData();
    }

    public int CalculateActorStat(BonusType type, int stat)
    {
        return (int)CalculateActorStat(type, (float)stat);
    }

    public float CalculateActorStat(BonusType type, float stat)
    {
        StatBonus bonus = new StatBonus();
        GetTotalStatBonus(type, groupTypes, null, bonus);
        return bonus.CalculateStat(stat);
    }

    protected void ApplyHealthBonuses()
    {
        float percentage = CurrentHealth / MaximumHealth;
        MaximumHealth = (int)CalculateActorStat(BonusType.MAX_HEALTH, BaseHealth);
        CurrentHealth = (MaximumHealth * percentage);

        float percentHealthRegen = CalculateActorStat(BonusType.PERCENT_HEALTH_REGEN, 0f) / 100f;

        HealthRegenRate = -(percentHealthRegen * MaximumHealth + CalculateActorStat(BonusType.HEALTH_REGEN, 0f));
    }

    protected void ApplySoulPointBonuses()
    {
        float percentage = CurrentSoulPoints / MaximumSoulPoints;
        MaximumSoulPoints = (int)CalculateActorStat(BonusType.MAX_SOULPOINTS, BaseSoulPoints);
        CurrentSoulPoints = (MaximumSoulPoints * percentage);
    }

    protected void ApplyResistanceBonuses()
    {
        ElementData.SetResistanceCap(ElementType.FIRE, (int)GetMultiStatBonus(null, groupTypes, BonusType.MAX_FIRE_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.COLD, (int)GetMultiStatBonus(null, groupTypes, BonusType.MAX_COLD_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.LIGHTNING, (int)GetMultiStatBonus(null, groupTypes, BonusType.MAX_LIGHTNING_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.EARTH, (int)GetMultiStatBonus(null, groupTypes, BonusType.MAX_EARTH_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.DIVINE, (int)GetMultiStatBonus(null, groupTypes, BonusType.MAX_DIVINE_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.VOID, (int)GetMultiStatBonus(null, groupTypes, BonusType.MAX_VOID_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));

        ElementData[ElementType.PHYSICAL] = (int)GetMultiStatBonus(null, groupTypes, BonusType.PHYSICAL_RESISTANCE).CalculateStat(0f);
        ElementData[ElementType.FIRE] = (int)GetMultiStatBonus(null, groupTypes, BonusType.FIRE_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.COLD] = (int)GetMultiStatBonus(null, groupTypes, BonusType.COLD_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.LIGHTNING] = (int)GetMultiStatBonus(null, groupTypes, BonusType.LIGHTNING_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.EARTH] = (int)GetMultiStatBonus(null, groupTypes, BonusType.EARTH_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.DIVINE] = (int)GetMultiStatBonus(null, groupTypes, BonusType.DIVINE_RESISTANCE, BonusType.PRIMORDIAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.VOID] = (int)GetMultiStatBonus(null, groupTypes, BonusType.VOID_RESISTANCE, BonusType.PRIMORDIAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
    }

    public void GetMultiStatBonus(StatBonus existingBonus, Dictionary<BonusType, StatBonus> abilityBonusProperties, IEnumerable<GroupType> tags, params BonusType[] types)
    {
        foreach (BonusType bonusType in types)
        {
            GetTotalStatBonus(bonusType, tags, abilityBonusProperties, existingBonus);
        }
    }

    public StatBonus GetMultiStatBonus(Dictionary<BonusType, StatBonus> abilityBonusProperties, IEnumerable<GroupType> tags, params BonusType[] types)
    {
        StatBonus bonus = new StatBonus();
        foreach (BonusType bonusType in types)
        {
            GetTotalStatBonus(bonusType, tags, abilityBonusProperties, bonus);
        }
        return bonus;
    }

    public virtual void UpdateActorData()
    {

        float percentShieldRegen = CalculateActorStat(BonusType.PERCENT_SHIELD_REGEN, 0f) / 100f;
        ShieldRegenRate = (percentShieldRegen * MaximumManaShield + CalculateActorStat(BonusType.SHIELD_REGEN, 0f));
        ShieldRestoreRate = (CalculateActorStat(BonusType.SHIELD_RESTORE_SPEED, 10f) / 100f * MaximumManaShield);
        ShieldRestoreDelayModifier = CalculateActorStat(BonusType.SHIELD_RESTORE_DELAY, 100f) / 100f;

        PhysicalNegation = GetMultiStatBonus(null, groupTypes, BonusType.PHYSICAL_RESISTANCE_NEGATION).CalculateStat(0);
        FireNegation = GetMultiStatBonus(null, groupTypes, BonusType.FIRE_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        ColdNegation = GetMultiStatBonus(null, groupTypes, BonusType.COLD_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        LightningNegation = GetMultiStatBonus(null, groupTypes, BonusType.LIGHTNING_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        EarthNegation = GetMultiStatBonus(null, groupTypes, BonusType.EARTH_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        DivineNegation = GetMultiStatBonus(null, groupTypes, BonusType.DIVINE_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0);
        VoidNegation = GetMultiStatBonus(null, groupTypes, BonusType.VOID_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0);
    }

    protected abstract HashSet<GroupType> GetGroupTypes();

    public abstract void GetTotalStatBonus(BonusType type, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus outBonus);

    public abstract int GetResistance(ElementType element);
}