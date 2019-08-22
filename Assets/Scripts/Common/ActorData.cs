using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorData
{
    public Guid Id { get; protected set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string Name { get; set; }
    public float BaseHealth { get; protected set; }

    [SerializeField]
    public int MaximumHealth { get; set; }

    [SerializeField]
    public float CurrentHealth { get; set; }

    public int MinimumHealth { get; protected set; } //for cases of invincible/phased actors

    public bool HealthIsHitsToKill { get; protected set; } //health is number of hits to kill

    public float BaseSoulPoints { get; protected set; }
    public int MaximumSoulPoints { get; set; }
    public float CurrentSoulPoints { get; set; }

    public int BaseManaShield { get; protected set; }
    public int MaximumManaShield { get; set; }
    public float CurrentManaShield { get; set; }

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

    public int PhysicalNegation { get; protected set; }
    public int FireNegation { get; protected set; }
    public int ColdNegation { get; protected set; }
    public int LightningNegation { get; protected set; }
    public int EarthNegation { get; protected set; }
    public int DivineNegation { get; protected set; }
    public int VoidNegation { get; protected set; }

    protected Dictionary<BonusType, StatBonus> statBonuses;
    protected Dictionary<BonusType, StatBonus> temporaryBonuses;

    protected ElementResistances Resistances { get; private set; }

    public float movementSpeed;

    protected ActorData()
    {
        Id = Guid.NewGuid();
        Resistances = new ElementResistances();
        statBonuses = new Dictionary<BonusType, StatBonus>();
        temporaryBonuses = new Dictionary<BonusType, StatBonus>();
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

    public void AddStatBonus(int value, BonusType type, ModifyType modifier)
    {
        if (!statBonuses.ContainsKey(type))
            statBonuses.Add(type, new StatBonus());
        statBonuses[type].AddBonus(modifier, value);
    }

    public void RemoveStatBonus(int value, BonusType type, ModifyType modifier)
    {
        statBonuses[type].RemoveBonus(modifier, value);
    }

    public void AddTemporaryBonus(int value, BonusType type, ModifyType modifier)
    {
        if (!temporaryBonuses.ContainsKey(type))
            temporaryBonuses.Add(type, new StatBonus());
        temporaryBonuses[type].AddBonus(modifier, value);
        UpdateActorData();
    }

    public void RemoveTemporaryBonus(int value, BonusType type, ModifyType modifier)
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
        GetTotalStatBonus(type, null, bonus);
        return bonus.CalculateStat(stat);
    }

    protected void ApplyHealthBonuses()
    {
        float percentage = CurrentHealth / MaximumHealth;
        MaximumHealth = (int)CalculateActorStat(BonusType.MAX_HEALTH, BaseHealth);
        CurrentHealth = (MaximumHealth * percentage);
    }

    protected void ApplySoulPointBonuses()
    {
        float percentage = CurrentSoulPoints / MaximumSoulPoints;
        MaximumSoulPoints = (int)CalculateActorStat(BonusType.MAX_SOULPOINTS, BaseSoulPoints);
        CurrentSoulPoints = (MaximumSoulPoints * percentage);
    }

    protected void ApplyResistanceBonuses()
    {
        Resistances.SetResistanceCap(ElementType.FIRE, (int)GetMultiStatBonus(null, BonusType.MAX_FIRE_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        Resistances.SetResistanceCap(ElementType.COLD, (int)GetMultiStatBonus(null, BonusType.MAX_COLD_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        Resistances.SetResistanceCap(ElementType.LIGHTNING, (int)GetMultiStatBonus(null, BonusType.MAX_LIGHTNING_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        Resistances.SetResistanceCap(ElementType.EARTH, (int)GetMultiStatBonus(null, BonusType.MAX_EARTH_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        Resistances.SetResistanceCap(ElementType.DIVINE, (int)GetMultiStatBonus(null, BonusType.MAX_DIVINE_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        Resistances.SetResistanceCap(ElementType.VOID, (int)GetMultiStatBonus(null, BonusType.MAX_VOID_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));

        Resistances[ElementType.PHYSICAL] = (int)GetMultiStatBonus(null, BonusType.PHYSICAL_RESISTANCE).CalculateStat(0f);
        Resistances[ElementType.FIRE] = (int)GetMultiStatBonus(null, BonusType.FIRE_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        Resistances[ElementType.COLD] = (int)GetMultiStatBonus(null, BonusType.COLD_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        Resistances[ElementType.LIGHTNING] = (int)GetMultiStatBonus(null, BonusType.LIGHTNING_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        Resistances[ElementType.EARTH] = (int)GetMultiStatBonus(null, BonusType.EARTH_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        Resistances[ElementType.DIVINE] = (int)GetMultiStatBonus(null, BonusType.DIVINE_RESISTANCE, BonusType.PRIMORDIAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        Resistances[ElementType.VOID] = (int)GetMultiStatBonus(null, BonusType.VOID_RESISTANCE, BonusType.PRIMORDIAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
    }

    public void GetMultiStatBonus(Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus existingBonus, params BonusType[] types)
    {
        foreach (BonusType bonusType in types)
        {
            GetTotalStatBonus(bonusType, abilityBonusProperties, existingBonus);
        }
    }

    public StatBonus GetMultiStatBonus(Dictionary<BonusType, StatBonus> abilityBonusProperties, params BonusType[] types)
    {
        StatBonus bonus = new StatBonus();
        foreach (BonusType bonusType in types)
        {
            GetTotalStatBonus(bonusType, abilityBonusProperties, bonus);
        }
        return bonus;
    }

    public virtual void UpdateActorData()
    {
        PhysicalNegation = GetMultiStatBonus(null, BonusType.PHYSICAL_RESISTANCE_NEGATION).CalculateStat(0);
        FireNegation = GetMultiStatBonus(null, BonusType.FIRE_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        ColdNegation = GetMultiStatBonus(null, BonusType.COLD_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        LightningNegation = GetMultiStatBonus(null, BonusType.LIGHTNING_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        EarthNegation = GetMultiStatBonus(null, BonusType.EARTH_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        DivineNegation = GetMultiStatBonus(null, BonusType.DIVINE_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0);
        VoidNegation = GetMultiStatBonus(null, BonusType.VOID_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0);
    }

    public abstract void GetTotalStatBonus(BonusType type, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus outBonus);

    public abstract int GetResistance(ElementType element);
}