using System.Collections.Generic;
using UnityEngine;

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
        foreach (TriggeredEffect abilityEffects in AbilityTriggeredEffects[triggerType])
        {
            abilityEffects.OnTrigger(target, SourceActor);
        }

        base.ApplyTriggerEffects(triggerType, target);
    }
}

public class OnHitDataContainer
{
    public virtual Actor SourceActor { get; set; }

    protected readonly Dictionary<EffectType, OnHitStatusEffectData> effectData;
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

    public virtual int GetNegation(ElementType e) => SourceActor.Data.GetNegation(e);

    public float GetEffectChance(EffectType type) => effectData[type].chance;

    public void SetEffectChance(EffectType type, float value) => effectData[type].chance = value;

    public float GetEffectEffectiveness(EffectType type) => effectData[type].effectiveness;

    public void SetEffectEffectiveness(EffectType type, float value) => effectData[type].effectiveness = value;

    public float GetEffectDuration(EffectType type) => effectData[type].duration;

    public void SetEffectDuration(EffectType type, float value) => effectData[type].duration = value;

    public OnHitStatusEffectData GetEffectData(EffectType type) => effectData[type];

    public virtual void ApplyTriggerEffects(TriggerType triggerType, Actor target)
    {
        foreach (TriggeredEffect actorEffect in SourceActor.Data.TriggeredEffects[triggerType])
        {
            actorEffect.OnTrigger(target, SourceActor);
        }
    }

    public bool DidEffectProc(EffectType effectType, int avoidance)
    {
        float chance = GetEffectChance(effectType);
        return Helpers.RollChance((chance - avoidance) / 100f);
    }

    public void UpdateStatusEffectData(ActorData data, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonus)
    {
        StatBonus bleedChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus bleedEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus bleedDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float bleedSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BLEED_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        SetEffectChance(EffectType.BLEED, bleedChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.BLEED, bleedEffectiveness.CalculateStat(100f) / 100f * bleedSpeed);
        SetEffectDuration(EffectType.BLEED, bleedDuration.CalculateStat(BleedEffect.BASE_DURATION) / bleedSpeed);

        StatBonus burnChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus burnEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus burnDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        float burnSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.BURN_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        SetEffectChance(EffectType.BURN, burnChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.BURN, burnEffectiveness.CalculateStat(100f) / 100f * burnSpeed);
        SetEffectDuration(EffectType.BURN, burnDuration.CalculateStat(BurnEffect.BASE_DURATION) / burnSpeed);

        StatBonus chillChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.CHILL_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus chillEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.CHILL_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus chillDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.CHILL_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        SetEffectChance(EffectType.CHILL, chillChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.CHILL, chillEffectiveness.CalculateStat(100f) / 100f);
        SetEffectDuration(EffectType.CHILL, chillDuration.CalculateStat(ChillEffect.BASE_DURATION));

        StatBonus electrocuteChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus electrocuteEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus electrocuteDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        float electrocuteSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.ELECTROCUTE_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        SetEffectChance(EffectType.ELECTROCUTE, electrocuteChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.ELECTROCUTE, electrocuteEffectiveness.CalculateStat(100f) / 100f * electrocuteSpeed);
        SetEffectDuration(EffectType.ELECTROCUTE, electrocuteDuration.CalculateStat(ElectrocuteEffect.BASE_DURATION) / electrocuteSpeed);

        StatBonus fractureChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.FRACTURE_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus fractureEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.FRACTURE_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus fractureDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.FRACTURE_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        SetEffectChance(EffectType.FRACTURE, fractureChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.FRACTURE, fractureEffectiveness.CalculateStat(100f) / 100f);
        SetEffectDuration(EffectType.FRACTURE, fractureDuration.CalculateStat(FractureEffect.BASE_DURATION));

        StatBonus pacifyChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.PACIFY_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.PRIMORDIAL_STATUS_EFFECT_CHANCE);
        StatBonus pacifyEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.PACIFY_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus pacifyDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.PACIFY_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.PRIMORDIAL_STATUS_EFFECT_DURATION);
        SetEffectChance(EffectType.PACIFY, pacifyChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.PACIFY, pacifyEffectiveness.CalculateStat(100f) / 100f);
        SetEffectDuration(EffectType.PACIFY, pacifyDuration.CalculateStat(PacifyEffect.BASE_DURATION));

        StatBonus radiationChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.PRIMORDIAL_STATUS_EFFECT_CHANCE);
        StatBonus radiationEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus radiationDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.PRIMORDIAL_STATUS_EFFECT_DURATION);
        float radiationSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.RADIATION_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        SetEffectChance(EffectType.RADIATION, radiationChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.RADIATION, radiationEffectiveness.CalculateStat(100f) / 100f * radiationSpeed);
        SetEffectDuration(EffectType.RADIATION, radiationDuration.CalculateStat(RadiationEffect.BASE_DURATION) / radiationSpeed);

        StatBonus poisonChance = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_CHANCE);
        StatBonus poisonEffectiveness = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus poisonDuration = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float poisonSpeed = data.GetMultiStatBonus(abilityBonus, tags, BonusType.POISON_SPEED, BonusType.DAMAGE_OVER_TIME_SPEED).CalculateStat(100f) / 100f;
        SetEffectChance(EffectType.POISON, poisonChance.CalculateStat(0f));
        SetEffectEffectiveness(EffectType.POISON, poisonEffectiveness.CalculateStat(100f) / 100f * poisonSpeed);
        SetEffectDuration(EffectType.POISON, poisonDuration.CalculateStat(PoisonEffect.BASE_DURATION) / poisonSpeed);

        this.vsBossDamage = data.GetMultiStatBonus(abilityBonus, tags, BonusType.DAMAGE_VS_BOSS).CalculateStat(1f);

        this.directHitDamage = data.GetMultiStatBonus(abilityBonus, tags, BonusType.DIRECT_HIT_DAMAGE).CalculateStat(1f);
    }

    public void ApplyEffectToTarget(Actor target, EffectType effectType, float effectPower)
    {
        ActorEffect.ApplyEffectToTarget(target, SourceActor, effectType, effectPower, GetEffectDuration(effectType));
    }

    public class OnHitStatusEffectData
    {
        public float chance = 0;
        public float effectiveness = 0;
        public float duration = 0;
    }
}