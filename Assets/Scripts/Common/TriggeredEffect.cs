using System;
using System.Collections.Generic;

public class TriggeredEffect
{
    public string sourceName;
    public TriggeredEffectBonusProperty BaseEffect { get; private set; }
    public float Value { get; private set; }

    public TriggeredEffect(TriggeredEffectBonusProperty baseEffect, float value, string sourceName)
    {
        this.sourceName = sourceName;
        BaseEffect = baseEffect;
        Value = value;
    }

    public bool RollTriggerChance()
    {
        return Helpers.RollChance(BaseEffect.triggerChance);
    }

    public void ModifyValue(float value)
    {
        Value += value;
    }

    public void OnTrigger(Actor target, Actor source)
    {
        if (!RollTriggerChance())
        {
            return;
        }

        switch (BaseEffect.effectTargetType)
        {
            case AbilityTargetType.SELF:
                target = source;
                ApplyEffect(target, source);
                return;

            case AbilityTargetType.ENEMY:
                ApplyEffect(target, source);
                return;

            case AbilityTargetType.ALLY:
            case AbilityTargetType.ALL:
            case AbilityTargetType.NONE:
                break;

            default:
                break;
        }
    }

    private void ApplyEffect(Actor target, Actor source)
    {
        switch (BaseEffect.effectType)
        {
            case EffectType.DEBUFF:
            case EffectType.BUFF:
                string buffName = sourceName + BaseEffect.statBonusType.ToString() + BaseEffect.statModifyType.ToString();
                ApplyBuffEffect(target, source, buffName);
                return;

            case EffectType.BLEED:
            case EffectType.BURN:
            case EffectType.CHILL:
            case EffectType.ELECTROCUTE:
            case EffectType.FRACTURE:
            case EffectType.PACIFY:
            case EffectType.RADIATION:
            case EffectType.POISON:
                float effectPowerMultiplier = source.Data.OnHitData.effectData[BaseEffect.effectType].Effectiveness;
                ActorEffect.ApplyEffectToTarget(target, source, BaseEffect.effectType, Value * effectPowerMultiplier * target.Data.AfflictedStatusDamageResistance, source.Data.OnHitData.effectData[BaseEffect.effectType].Duration);
                break;

            default:
                ActorEffect.ApplyEffectToTarget(target, source, BaseEffect.effectType, Value, BaseEffect.effectDuration, 1.0f, BaseEffect.effectElement);
                return;
        }
    }

    public void ApplyBuffEffect(Actor target, Actor source, string buffName)
    {
        List<SourcedActorEffect> buffs = target.GetBuffStatusEffect(buffName);

        if (buffs.Count > 0)
        {
            return;
        }

        List<TempEffectBonusContainer.StatusBonus> bonuses = new List<TempEffectBonusContainer.StatusBonus>
            {
                new TempEffectBonusContainer.StatusBonus(BaseEffect.statBonusType, BaseEffect.statModifyType, Value, BaseEffect.effectDuration)
            };

        target.AddStatusEffect(new StatBonusBuffEffect(target, source, bonuses, BaseEffect.effectDuration, buffName, BaseEffect.effectType, 1f));
    }
}