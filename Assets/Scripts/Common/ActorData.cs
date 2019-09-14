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
    public float DamageTakenModifier { get; protected set; }
    public float AfflictedStatusDamageResistance { get; protected set; }
    public float AfflictedStatusThreshold { get; protected set; }
    public int AfflictedStatusAvoidance { get; protected set; }
    public float AfflictedStatusDuration { get; protected set; }

    public int PhysicalNegation { get => ElementData.GetNegation(ElementType.PHYSICAL); private set => ElementData.SetNegation(ElementType.PHYSICAL, value); }
    public int FireNegation { get => ElementData.GetNegation(ElementType.FIRE); private set => ElementData.SetNegation(ElementType.FIRE, value); }
    public int ColdNegation { get => ElementData.GetNegation(ElementType.COLD); private set => ElementData.SetNegation(ElementType.COLD, value); }
    public int LightningNegation { get => ElementData.GetNegation(ElementType.LIGHTNING); private set => ElementData.SetNegation(ElementType.LIGHTNING, value); }
    public int EarthNegation { get => ElementData.GetNegation(ElementType.EARTH); private set => ElementData.SetNegation(ElementType.EARTH, value); }
    public int DivineNegation { get => ElementData.GetNegation(ElementType.DIVINE); private set => ElementData.SetNegation(ElementType.DIVINE, value); }
    public int VoidNegation { get => ElementData.GetNegation(ElementType.VOID); private set => ElementData.SetNegation(ElementType.VOID, value); }

    public List<TriggeredEffect> WhenHittingEffects { get; protected set; }
    public List<TriggeredEffect> WhenHitEffects { get; protected set; }
    public List<TriggeredEffect> OnHitEffects { get; protected set; }
    public List<TriggeredEffect> OnKillEffects { get; protected set; }

    protected Dictionary<BonusType, StatBonusCollection> statBonuses;
    protected Dictionary<BonusType, StatBonus> temporaryBonuses;

    protected ElementalData ElementData { get; private set; }

    public HashSet<GroupType> GroupTypes { get; protected set; }

    public float movementSpeed;

    protected ActorData()
    {
        Id = Guid.NewGuid();
        ElementData = new ElementalData();
        statBonuses = new Dictionary<BonusType, StatBonusCollection>();
        temporaryBonuses = new Dictionary<BonusType, StatBonus>();
        GroupTypes = new HashSet<GroupType>();
        WhenHittingEffects = new List<TriggeredEffect>();
        WhenHitEffects = new List<TriggeredEffect>();
        OnHitEffects = new List<TriggeredEffect>();
        OnKillEffects = new List<TriggeredEffect>();
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

    public bool RemoveStatBonus(BonusType type, GroupType restriction, ModifyType modifier, float value)
    {
        return statBonuses[type].RemoveBonus(restriction, modifier, value);
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
        temporaryBonuses[type].RemoveBonus(modifier, value);
        if (!deferUpdate)
            UpdateActorData();
    }

    public void ClearTemporaryBonuses()
    {
        temporaryBonuses.Clear();
        UpdateActorData();
    }

    protected void ApplyHealthBonuses()
    {
        float percentage = CurrentHealth / MaximumHealth;
        MaximumHealth = (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_HEALTH).CalculateStat(BaseHealth);
        CurrentHealth = MaximumHealth * percentage;

        float percentHealthRegen = GetMultiStatBonus(GroupTypes, BonusType.PERCENT_HEALTH_REGEN).CalculateStat(0f) / 100f;
        HealthRegenRate = percentHealthRegen * MaximumHealth + GetMultiStatBonus(GroupTypes, BonusType.HEALTH_REGEN).CalculateStat(0f);
    }

    protected void ApplySoulPointBonuses()
    {
        float percentage = CurrentSoulPoints / MaximumSoulPoints;
        MaximumSoulPoints = (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_SOULPOINTS).CalculateStat(BaseSoulPoints);
        CurrentSoulPoints = MaximumSoulPoints * percentage;
    }

    protected void ApplyResistanceBonuses()
    {
        ElementData.SetResistanceCap(ElementType.FIRE, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_FIRE_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.COLD, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_COLD_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.LIGHTNING, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_LIGHTNING_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.EARTH, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_EARTH_RESISTANCE, BonusType.MAX_ELEMENTAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.DIVINE, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_DIVINE_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));
        ElementData.SetResistanceCap(ElementType.VOID, (int)GetMultiStatBonus(GroupTypes, BonusType.MAX_VOID_RESISTANCE, BonusType.MAX_PRIMORDIAL_RESISTANCES, BonusType.MAX_ALL_NONPHYSICAL_RESISTANCES)
            .CalculateStat(80f));

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
        ShieldRegenRate = percentShieldRegen * MaximumManaShield + GetMultiStatBonus(GroupTypes, BonusType.SHIELD_REGEN).CalculateStat(0f);
        ShieldRestoreRate = GetMultiStatBonus(GroupTypes, BonusType.SHIELD_RESTORE_SPEED).CalculateStat(0.1f) * MaximumManaShield;
        ShieldRestoreDelayModifier = GetMultiStatBonus(GroupTypes, BonusType.SHIELD_RESTORE_DELAY).CalculateStat(1f);
        DamageTakenModifier = GetMultiStatBonus(GroupTypes, BonusType.DAMAGE_TAKEN).CalculateStat(100f);

        PhysicalNegation = GetMultiStatBonus(GroupTypes, BonusType.PHYSICAL_RESISTANCE_NEGATION).CalculateStat(0);
        FireNegation = GetMultiStatBonus(GroupTypes, BonusType.FIRE_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        ColdNegation = GetMultiStatBonus(GroupTypes, BonusType.COLD_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        LightningNegation = GetMultiStatBonus(GroupTypes, BonusType.LIGHTNING_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        EarthNegation = GetMultiStatBonus(GroupTypes, BonusType.EARTH_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION).CalculateStat(0);
        DivineNegation = GetMultiStatBonus(GroupTypes, BonusType.DIVINE_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0);
        VoidNegation = GetMultiStatBonus(GroupTypes, BonusType.VOID_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION).CalculateStat(0);

        AfflictedStatusAvoidance = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_AVOIDANCE).CalculateStat(0);
        AfflictedStatusDuration = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_DURATION).CalculateStat(1f);
    }

    protected abstract HashSet<GroupType> GetGroupTypes();

    public abstract void GetTotalStatBonus(BonusType type, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus outBonus);

    public abstract int GetResistance(ElementType element);

    public class TriggeredEffect
    {
        public TriggeredEffectBonusProperty BaseEffect { get; private set; }
        public float Value { get; private set; }

        public TriggeredEffect(TriggeredEffectBonusProperty baseEffect, float value)
        {
            this.BaseEffect = baseEffect;
            this.Value = value;
        }

        public bool RollTriggerChance()
        {
            
            return Helpers.RollChance(BaseEffect.triggerChance);
        }
    }
}