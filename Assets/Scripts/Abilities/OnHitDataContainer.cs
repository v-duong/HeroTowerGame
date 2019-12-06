using System.Collections.Generic;

public class AbilityOnHitDataContainer : OnHitDataContainer
{
    public ActorAbility sourceAbility;
    public AbilityType Type => sourceAbility.abilityBase.abilityType;
    public override Actor SourceActor => sourceAbility.AbilityOwner;
    public Dictionary<TriggerType, List<TriggeredEffect>> AbilityTriggeredEffects => sourceAbility.triggeredEffects;

    private readonly Dictionary<ElementType, int> negations;
    public float accuracy;

    public AbilityOnHitDataContainer(ActorAbility sourceAbility) : base()
    {
        this.sourceAbility = sourceAbility;
        negations = new Dictionary<ElementType, int>();
    }

    public override int GetNegation(ElementType e) => negations[e];

    public void SetNegation(ElementType e, int value) => negations[e] = value;

    public override void ApplyTriggerEffects(TriggerType triggerType, Actor target)
    {
        foreach (TriggeredEffect abilityEffects in AbilityTriggeredEffects[triggerType].ToArray())
        {
            abilityEffects.OnTrigger(target, SourceActor);
        }

        base.ApplyTriggerEffects(triggerType, target);
    }
}

public class OnHitDataContainer
{
    public virtual Actor SourceActor { get; set; }

    public Dictionary<EffectType, OnHitStatusEffectData> effectData;
    public float vsBossDamage;
    public float directHitDamage;

    public OnHitDataContainer()
    {
        effectData = new Dictionary<EffectType, OnHitStatusEffectData>
        {
            { EffectType.BLEED, new OnHitStatusEffectData() },
            { EffectType.BURN, new OnHitStatusEffectData() },
            { EffectType.CHILL, new OnHitStatusEffectData() },
            { EffectType.ELECTROCUTE, new OnHitStatusEffectData() },
            { EffectType.FRACTURE, new OnHitStatusEffectData() },
            { EffectType.PACIFY, new OnHitStatusEffectData() },
            { EffectType.RADIATION, new OnHitStatusEffectData() },
            { EffectType.POISON, new OnHitStatusEffectData() }
        };
    }

    public OnHitDataContainer DeepClone()
    {
        OnHitDataContainer newContainer = (OnHitDataContainer)this.MemberwiseClone();
        newContainer.effectData = new Dictionary<EffectType, OnHitStatusEffectData>
        {
            { EffectType.BLEED, effectData[EffectType.BLEED].Clone() },
            { EffectType.BURN, effectData[EffectType.BURN].Clone() },
            { EffectType.CHILL, effectData[EffectType.CHILL].Clone() },
            { EffectType.ELECTROCUTE, effectData[EffectType.ELECTROCUTE].Clone() },
            { EffectType.FRACTURE, effectData[EffectType.FRACTURE].Clone() },
            { EffectType.PACIFY, effectData[EffectType.PACIFY].Clone() },
            { EffectType.RADIATION, effectData[EffectType.RADIATION].Clone() },
            { EffectType.POISON, effectData[EffectType.POISON].Clone() }
        };

        return newContainer;
    }

    public virtual int GetNegation(ElementType e) => SourceActor.Data.GetNegation(e);

    public virtual void ApplyTriggerEffects(TriggerType triggerType, Actor target)
    {
        Dictionary<ElementType, MinMaxRange> retaliationToEnemy = new Dictionary<ElementType, MinMaxRange>();
        Dictionary<ElementType, MinMaxRange> retaliationToSelf = new Dictionary<ElementType, MinMaxRange>();
        foreach (TriggeredEffect actorEffect in SourceActor.Data.TriggeredEffects[triggerType].ToArray())
        {
            TriggeredEffectBonusProperty baseEffect = actorEffect.BaseEffect;
            if (baseEffect.effectType == EffectType.RETALIATION_DAMAGE)
            {
                if (baseEffect.effectTargetType == AbilityTargetType.ENEMY)
                {
                    if (!retaliationToEnemy.ContainsKey(baseEffect.effectElement))
                        retaliationToEnemy.Add(baseEffect.effectElement, new MinMaxRange());
                    retaliationToEnemy[baseEffect.effectElement].AddToBoth((int)actorEffect.Value);
                }
                else if (baseEffect.effectTargetType == AbilityTargetType.SELF)
                {
                    if (!retaliationToSelf.ContainsKey(baseEffect.effectElement))
                        retaliationToSelf.Add(baseEffect.effectElement, new MinMaxRange());
                    retaliationToSelf[baseEffect.effectElement].AddToBoth((int)actorEffect.Value);
                }
                continue;
            }

            actorEffect.OnTrigger(target, SourceActor);
        }
        if (retaliationToEnemy.Count > 0)
            SourceActor.StartCoroutine(InstantEffects.ApplyRetaliationDamageEffect(target, SourceActor, retaliationToEnemy));

        if (retaliationToSelf.Count > 0)
            SourceActor.StartCoroutine(InstantEffects.ApplyRetaliationDamageEffect(SourceActor, SourceActor, retaliationToSelf));
    }

    public bool DidEffectProc(EffectType effectType, int avoidance)
    {
        float chance = effectData[effectType].Chance;
        return Helpers.RollChance((chance - avoidance) / 100f);
    }

    public void UpdateStatusEffectData(ActorData data, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonus)
    {
        StatBonus bleedChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus bleedEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus bleedDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float bleedSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        effectData[EffectType.BLEED].SetChance(bleedChance.CalculateStat(0f));
        effectData[EffectType.BLEED].SetEffectiveness(bleedEffectiveness.CalculateStat(100f) / 100f * bleedSpeed);
        effectData[EffectType.BLEED].SetDuration(bleedDuration.CalculateStat(BleedEffect.BASE_DURATION) / bleedSpeed);
        effectData[EffectType.BLEED].SetMaxStacks(data.GetMultiStatBonus(abilityBonus, tags, BonusType.MAX_BLEED_STACKS).CalculateStat(1));
        effectData[EffectType.BLEED].SetSecondaryEffectiveness(data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_BONUS_DAMAGE_OVER_DISTANCE).CalculateStat(1f));
        effectData[EffectType.BLEED].SetResistance(1 - data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_RESISTANCE).CalculateStat(0f) / 100f);

        StatBonus burnChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus burnEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus burnDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        float burnSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        effectData[EffectType.BURN].SetChance(burnChance.CalculateStat(0f));
        effectData[EffectType.BURN].SetEffectiveness(burnEffectiveness.CalculateStat(100f) / 100f * burnSpeed);
        effectData[EffectType.BURN].SetDuration(burnDuration.CalculateStat(BurnEffect.BASE_DURATION) / burnSpeed);
        effectData[EffectType.BURN].SetMaxStacks(data.GetMultiStatBonus(abilityBonus, tags, BonusType.MAX_BURN_STACKS).CalculateStat(1));
        effectData[EffectType.BURN].SetResistance(1 - data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_RESISTANCE).CalculateStat(0f) / 100f);

        StatBonus chillChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.CHILL_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus chillEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.CHILL_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus chillDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.CHILL_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        effectData[EffectType.CHILL].SetChance(chillChance.CalculateStat(0f));
        effectData[EffectType.CHILL].SetEffectiveness(chillEffectiveness.CalculateStat(100f) / 100f);
        effectData[EffectType.CHILL].SetDuration(chillDuration.CalculateStat(ChillEffect.BASE_DURATION));

        StatBonus electrocuteChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus electrocuteEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus electrocuteDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        float electrocuteSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        effectData[EffectType.ELECTROCUTE].SetChance(electrocuteChance.CalculateStat(0f));
        effectData[EffectType.ELECTROCUTE].SetEffectiveness(electrocuteEffectiveness.CalculateStat(100f) / 100f * electrocuteSpeed);
        effectData[EffectType.ELECTROCUTE].SetDuration(electrocuteDuration.CalculateStat(ElectrocuteEffect.BASE_DURATION) / electrocuteSpeed);
        effectData[EffectType.ELECTROCUTE].SetMaxStacks(data.GetMultiStatBonus(abilityBonus, tags, BonusType.MAX_ELECTROCUTE_STACKS).CalculateStat(1));
        effectData[EffectType.ELECTROCUTE].SetResistance(1 - data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_RESISTANCE).CalculateStat(0f) / 100f);

        StatBonus fractureChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.FRACTURE_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus fractureEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.FRACTURE_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus fractureDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.FRACTURE_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        effectData[EffectType.FRACTURE].SetChance(fractureChance.CalculateStat(0f));
        effectData[EffectType.FRACTURE].SetEffectiveness(fractureEffectiveness.CalculateStat(100f) / 100f);
        effectData[EffectType.FRACTURE].SetDuration(fractureDuration.CalculateStat(FractureEffect.BASE_DURATION));

        StatBonus pacifyChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.PACIFY_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.PRIMORDIAL_STATUS_EFFECT_CHANCE);
        StatBonus pacifyEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.PACIFY_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus pacifyDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.PACIFY_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.PRIMORDIAL_STATUS_EFFECT_DURATION);
        effectData[EffectType.PACIFY].SetChance(pacifyChance.CalculateStat(0f));
        effectData[EffectType.PACIFY].SetEffectiveness(pacifyEffectiveness.CalculateStat(100f) / 100f);
        effectData[EffectType.PACIFY].SetDuration(pacifyDuration.CalculateStat(PacifyEffect.BASE_DURATION));

        StatBonus radiationChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.PRIMORDIAL_STATUS_EFFECT_CHANCE);
        StatBonus radiationEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus radiationDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.PRIMORDIAL_STATUS_EFFECT_DURATION);
        float radiationSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        effectData[EffectType.RADIATION].SetChance(radiationChance.CalculateStat(0f));
        effectData[EffectType.RADIATION].SetEffectiveness(radiationEffectiveness.CalculateStat(100f) / 100f * radiationSpeed);
        effectData[EffectType.RADIATION].SetDuration(radiationDuration.CalculateStat(RadiationEffect.BASE_DURATION) / radiationSpeed);
        effectData[EffectType.RADIATION].SetMaxStacks(data.GetMultiStatBonus(abilityBonus, tags, BonusType.MAX_RADIATION_STACKS).CalculateStat(1));
        effectData[EffectType.RADIATION].SetResistance(1 - data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_RESISTANCE).CalculateStat(0f) / 100f);

        StatBonus poisonChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_CHANCE);
        StatBonus poisonEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus poisonDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float poisonSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        effectData[EffectType.POISON].SetChance(poisonChance.CalculateStat(0f));
        effectData[EffectType.POISON].SetEffectiveness(poisonEffectiveness.CalculateStat(100f) / 100f * poisonSpeed);
        effectData[EffectType.POISON].SetDuration(poisonDuration.CalculateStat(PoisonEffect.BASE_DURATION) / poisonSpeed);
        effectData[EffectType.POISON].SetMaxStacks(data.GetMultiStatBonus(abilityBonus, tags, BonusType.MAX_POISON_STACKS).CalculateStat(PoisonEffect.BASE_MAX_STACKS));
        effectData[EffectType.POISON].SetResistance(1 - data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_RESISTANCE).CalculateStat(0f) / 100f);

        this.vsBossDamage = data.GetMultiStatBonus(abilityBonus, tags, BonusType.DAMAGE_VS_BOSS).CalculateStat(1f);

        this.directHitDamage = data.GetMultiStatBonus(abilityBonus, tags, BonusType.DIRECT_HIT_DAMAGE).CalculateStat(1f);
    }

    public void ApplyEffectToTarget(Actor target, EffectType effectType, float effectPower)
    {
        ActorEffect.ApplyEffectToTarget(target, SourceActor, effectType, effectPower, effectData[effectType].Duration);
    }

    public class OnHitStatusEffectData
    {
        public float Chance { get; private set; }
        public float Effectiveness { get; private set; }
        public float Duration { get; private set; }
        public int MaxStacks { get; private set; }
        public float SecondaryEffectiveness { get; private set; }
        public float Resistance { get; private set; }

        public void SetChance(float value)
        {
            Chance = value;
        }

        public void SetEffectiveness(float value)
        {
            Effectiveness = value;
        }

        public void SetDuration(float value)
        {
            Duration = value;
        }

        public void SetMaxStacks(int value)
        {
            MaxStacks = value;
        }

        public void SetSecondaryEffectiveness(float value)
        {
            SecondaryEffectiveness = value;
        }

        public void SetResistance(float value)
        {
            Resistance = value;
        }

        public OnHitStatusEffectData()
        {
            MaxStacks = 1;
            SecondaryEffectiveness = 1f;
        }

        public OnHitStatusEffectData Clone()
        {
            return (OnHitStatusEffectData)MemberwiseClone();
        }
    }
}