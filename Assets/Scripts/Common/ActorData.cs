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

    protected Dictionary<BonusType, StatBonus> statBonuses;
    protected Dictionary<BonusType, StatBonus> temporaryBonuses;

    public ElementResistances Resistances { get; protected set; }

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
        return (int)Math.Round(CalculateActorStat(type, (double)stat), MidpointRounding.AwayFromZero);
    }

    public double CalculateActorStat(BonusType type, double stat)
    {
        StatBonus bonus = new StatBonus();
        GetTotalStatBonus(type, null, bonus);
        return bonus.CalculateStat(stat);
    }

    public abstract void UpdateActorData();
    public abstract void GetTotalStatBonus(BonusType type, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus bonus);
}