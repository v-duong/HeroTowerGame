using System;
using System.Collections.Generic;
using UnityEngine;

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

    public void OnTrigger(Actor target, Actor source)
    {
        if (!RollTriggerChance())
            return;

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
                default:
                    ActorEffect.ApplyEffectToTarget(target, source, BaseEffect.effectType, Value, BaseEffect.effectDuration, 1.0f, BaseEffect.effectElement);
                    return;
            }
    }

    public void ApplyBuffEffect(Actor target, Actor source, string buffName)
    {
        List<StatBonusBuffEffect> buffs = target.GetBuffStatusEffect(buffName);

        if (buffs.Count > 0)
        {
            return;
        }

        List<Tuple<BonusType, ModifyType, float>> bonuses = new List<Tuple<BonusType, ModifyType, float>>
            {
                new Tuple<BonusType, ModifyType, float>(BaseEffect.statBonusType, BaseEffect.statModifyType, Value)
            };

        target.AddStatusEffect(new StatBonusBuffEffect(target, source, bonuses, BaseEffect.effectDuration, buffName, BaseEffect.effectType));
    }
}