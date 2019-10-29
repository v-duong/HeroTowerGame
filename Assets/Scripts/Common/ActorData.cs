using System;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class ActorData
{
    public const float BLOCK_CHANCE_CAP = 80f;
    public const float BLOCK_PROTECTION_CAP = 90f;

    public const float ATTACK_PARRY_CAP = 40f;
    public const float SPELL_PARRY_CAP = 40f;

    protected const float DUAL_WIELD_ATTACK_SPEED_BONUS = 10f;

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
    public int BaseSpellPhasing { get; protected set; }
    public int BaseResolveRating { get; protected set; }

    public int Armor { get; protected set; }
    public int DodgeRating { get; protected set; }
    public int ResolveRating { get; protected set; }
    public int AttackPhasing { get; protected set; }
    public int SpellPhasing { get; protected set; }
    public float DamageTakenModifier { get; protected set; }
    public float AfflictedStatusDamageResistance { get; protected set; }
    public float AfflictedStatusThreshold { get; protected set; }
    public int AfflictedStatusAvoidance { get; protected set; }
    public float AfflictedStatusDuration { get; protected set; }

    public float BlockChance { get; protected set; }
    public float BlockProtection { get; protected set; }

    public float AttackParryChance { get; protected set; }
    public float SpellParryChance { get; protected set; }

    public float PoisonResistance { get; protected set; }

    public bool RechargeCannotBeStopped { get; set; }

    public float AggroPriorityModifier { get; protected set; }

    public int GetNegation(ElementType e) => ElementData.GetNegation(e);

    public void SetNegation(ElementType e, int value) => ElementData.SetNegation(e, value);

    public OnHitDataContainer OnHitData { get; private set; }

    public Dictionary<TriggerType, List<TriggeredEffect>> TriggeredEffects { get; protected set; }

    protected Dictionary<BonusType, StatBonusCollection> statBonuses;
    protected Dictionary<BonusType, StatBonus> selfBuffBonuses;
    protected Dictionary<BonusType, StatBonus> temporaryBonuses;
    protected Dictionary<BonusType, int> specialBonuses;

    protected ElementalData ElementData { get; private set; }

    public HashSet<GroupType> GroupTypes { get; protected set; }

    public float movementSpeed;

    protected ActorData()
    {
        Id = Guid.NewGuid();
        ElementData = new ElementalData();
        statBonuses = new Dictionary<BonusType, StatBonusCollection>();
        AddStatBonus(BonusType.GLOBAL_ATTACK_SPEED, GroupType.DUAL_WIELD, ModifyType.MULTIPLY, DUAL_WIELD_ATTACK_SPEED_BONUS);
        temporaryBonuses = new Dictionary<BonusType, StatBonus>();
        specialBonuses = new Dictionary<BonusType, int>();
        GroupTypes = new HashSet<GroupType>();
        TriggeredEffects = new Dictionary<TriggerType, List<TriggeredEffect>>();
        foreach (TriggerType t in Enum.GetValues(typeof(TriggerType)))
        {
            TriggeredEffects.Add(t, new List<TriggeredEffect>());
        }
        OnHitData = new OnHitDataContainer();
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
        if (type >= (BonusType)HeroArchetypeData.SpecialBonusStart)
        {
            AddSpecialBonus(type);
            return;
        }

        if (!statBonuses.ContainsKey(type))
            statBonuses.Add(type, new StatBonusCollection());
        statBonuses[type].AddBonus(restriction, modifier, value);
    }

    public bool RemoveStatBonus(BonusType type, GroupType restriction, ModifyType modifier, float value)
    {
        if (type >= (BonusType)HeroArchetypeData.SpecialBonusStart)
        {
            RemoveSpecialBonus(type);
            return true;
        }
        bool isRemoved = statBonuses[type].RemoveBonus(restriction, modifier, value);
        if (statBonuses[type].IsEmpty())
        {
            statBonuses.Remove(type);
        }
        return isRemoved;
    }

    public void AddTemporaryBonus(float value, BonusType type, ModifyType modifier, bool deferUpdate)
    {
        if (!temporaryBonuses.ContainsKey(type))
            temporaryBonuses.Add(type, new StatBonus());
        temporaryBonuses[type].AddBonus(modifier, value);
        if (!deferUpdate)
            UpdateActorData();
    }

    public void RemoveTemporaryBonus(float value, BonusType type, ModifyType modifier, bool deferUpdate)
    {
        if (temporaryBonuses.ContainsKey(type))
            temporaryBonuses[type].RemoveBonus(modifier, value);
        if (!deferUpdate)
            UpdateActorData();
    }

    public void AddSpecialBonus(BonusType type)
    {
        if (!specialBonuses.ContainsKey(type))
            specialBonuses.Add(type, 0);
        specialBonuses[type]++;
        UpdateActorData();
    }

    public void RemoveSpecialBonus(BonusType type)
    {
        if (!specialBonuses.ContainsKey(type))
            return;

        specialBonuses[type]--;

        if (specialBonuses[type] == 0)
            specialBonuses.Remove(type);

        UpdateActorData();
    }

    public bool HasSpecialBonus(BonusType type)
    {
        return specialBonuses.ContainsKey(type) && specialBonuses[type] > 0;
    }

    public void ClearTemporaryBonuses(bool updateActorData)
    {
        temporaryBonuses.Clear();
        if (updateActorData)
            UpdateActorData();
    }

    protected void ApplyHealthBonuses()
    {
        float percentage = CurrentHealth / MaximumHealth;
        MaximumHealth = (int)Math.Max(GetMultiStatBonus(GroupTypes, BonusType.MAX_HEALTH).CalculateStat(BaseHealth), 1);
        if (MaximumHealth > 1)
        {
            float convertedShieldPercent = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.MAX_HEALTH_TO_SHIELD).CalculateStat(0), 95) / 100f;
            BaseManaShield += (int)(MaximumHealth * convertedShieldPercent);
            MaximumHealth -= BaseManaShield;
        }
        CurrentHealth = MaximumHealth * percentage;

        float percentHealthRegen = GetMultiStatBonus(GroupTypes, BonusType.PERCENT_HEALTH_REGEN).CalculateStat(0f) / 100f;
        HealthRegenRate = GetMultiStatBonus(GroupTypes, BonusType.HEALTH_REGEN).CalculateStat(percentHealthRegen * MaximumHealth);
    }

    protected void ApplySoulPointBonuses()
    {
        float percentage = CurrentSoulPoints / MaximumSoulPoints;
        MaximumSoulPoints = (int)Math.Max(GetMultiStatBonus(GroupTypes, BonusType.MAX_SOULPOINTS).CalculateStat(BaseSoulPoints), 0);
        CurrentSoulPoints = MaximumSoulPoints * percentage;
    }

    protected void ApplyResistanceBonuses()
    {
        ElementData.SetResistanceCap(ElementType.FIRE, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_FIRE_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(ElementalData.DEFAULT_RESISTANCE_CAP));
        ElementData.SetResistanceCap(ElementType.COLD, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_COLD_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(ElementalData.DEFAULT_RESISTANCE_CAP));
        ElementData.SetResistanceCap(ElementType.LIGHTNING, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_LIGHTNING_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(ElementalData.DEFAULT_RESISTANCE_CAP));
        ElementData.SetResistanceCap(ElementType.EARTH, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_EARTH_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(ElementalData.DEFAULT_RESISTANCE_CAP));
        ElementData.SetResistanceCap(ElementType.DIVINE, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_DIVINE_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(ElementalData.DEFAULT_RESISTANCE_CAP));
        ElementData.SetResistanceCap(ElementType.VOID, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_VOID_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(ElementalData.DEFAULT_RESISTANCE_CAP));

        ElementData[ElementType.PHYSICAL] = (int)GetMultiStatBonus(GroupTypes, BonusType.PHYSICAL_RESISTANCE).CalculateStat(0f);
        ElementData[ElementType.FIRE] = (int)GetMultiStatBonus(GroupTypes, BonusType.FIRE_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.COLD] = (int)GetMultiStatBonus(GroupTypes, BonusType.COLD_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.LIGHTNING] = (int)GetMultiStatBonus(GroupTypes, BonusType.LIGHTNING_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.EARTH] = (int)GetMultiStatBonus(GroupTypes, BonusType.EARTH_RESISTANCE, BonusType.ELEMENTAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.DIVINE] = (int)GetMultiStatBonus(GroupTypes, BonusType.DIVINE_RESISTANCE, BonusType.PRIMORDIAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
        ElementData[ElementType.VOID] = (int)GetMultiStatBonus(GroupTypes, BonusType.VOID_RESISTANCE, BonusType.PRIMORDIAL_RESISTANCES, BonusType.ALL_NONPHYSICAL_RESISTANCES).CalculateStat(0f);
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

    public StatBonus GetMultiStatBonus(IEnumerable<GroupType> tags, params BonusType[] types)
    {
        StatBonus bonus = new StatBonus();
        foreach (BonusType bonusType in types)
        {
            GetTotalStatBonus(bonusType, tags, null, bonus);
        }
        return bonus;
    }

    public virtual void UpdateActorData()
    {
        float percentShieldRegen = GetMultiStatBonus(GroupTypes, BonusType.PERCENT_SHIELD_REGEN).CalculateStat(0f) / 100f;

        if (HasSpecialBonus(BonusType.SHIELD_RESTORE_APPLIES_TO_REGEN))
            ShieldRegenRate = GetMultiStatBonus(GroupTypes, BonusType.SHIELD_REGEN, BonusType.SHIELD_RESTORE_SPEED).CalculateStat(percentShieldRegen * MaximumManaShield);
        else
            ShieldRegenRate = GetMultiStatBonus(GroupTypes, BonusType.SHIELD_REGEN).CalculateStat(percentShieldRegen * MaximumManaShield);

        ShieldRestoreRate = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.SHIELD_RESTORE_SPEED).CalculateStat(0.1f), 0f) * MaximumManaShield;
        ShieldRestoreDelayModifier = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.SHIELD_RESTORE_DELAY).CalculateStat(1f), 0.05f);
        DamageTakenModifier = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.DAMAGE_TAKEN).CalculateStat(1f), 0.01f);

        ApplyResistanceBonuses();
        SetNegation(ElementType.PHYSICAL, GetMultiStatBonus(GroupTypes, BonusType.PHYSICAL_RESISTANCE_NEGATION).CalculateStat(0));
        SetNegation(ElementType.FIRE, GetMultiStatBonus(GroupTypes, BonusType.FIRE_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0));
        SetNegation(ElementType.COLD, GetMultiStatBonus(GroupTypes, BonusType.COLD_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0));
        SetNegation(ElementType.LIGHTNING, GetMultiStatBonus(GroupTypes, BonusType.LIGHTNING_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0));
        SetNegation(ElementType.EARTH, GetMultiStatBonus(GroupTypes, BonusType.EARTH_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0));
        SetNegation(ElementType.DIVINE, GetMultiStatBonus(GroupTypes, BonusType.DIVINE_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0));
        SetNegation(ElementType.VOID, GetMultiStatBonus(GroupTypes, BonusType.VOID_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0));

        AfflictedStatusAvoidance = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_AVOIDANCE).CalculateStat(0);
        AfflictedStatusDuration = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_DURATION).CalculateStat(1f), 0.01f);
        PoisonResistance = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.POISON_SHIELD_RESISTANCE).CalculateStat(0), 100) / 100f;

        AggroPriorityModifier = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.AGGRO_PRIORITY_RATE).CalculateStat(1f), 0);

        OnHitData.UpdateStatusEffectData(this, GetGroupTypes(), null);
    }

    protected abstract HashSet<GroupType> GetGroupTypes();

    public abstract void GetTotalStatBonus(BonusType type, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus outBonus);

    public abstract int GetResistance(ElementType element);

    public abstract HashSet<BonusType> BonusesIntersection(IEnumerable<BonusType> abilityBonuses, IEnumerable<BonusType> bonuses);

    public int GetAbilityLevel()
    {
        if (Level != 100)
            return (int)((Level - 1) / 2d);
        else
            return 50;
    }
}