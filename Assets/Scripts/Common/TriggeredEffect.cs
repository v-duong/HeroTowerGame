using System;
using System.Collections.Generic;

public class TriggeredEffect
{
    public TriggeredEffectBonusProperty BaseEffect { get; private set; }
    public float Value { get; private set; }

    public TriggeredEffect(TriggeredEffectBonusProperty baseEffect, float value)
    {
        BaseEffect = baseEffect;
        Value = value;
    }

    public bool RollTriggerChance()
    {
        return Helpers.RollChance(BaseEffect.triggerChance);
    }

    public void TriggerEffect(Actor target, Actor source, AbilityBase sourceAbility)
    {
        switch (BaseEffect.effectTargetType)
        {
            case AbilityTargetType.SELF:
                target = source;
                break;

            case AbilityTargetType.ENEMY:
                break;

            case AbilityTargetType.ALLY:
            case AbilityTargetType.ALL:
            case AbilityTargetType.NONE:
                break;
        }

        switch (BaseEffect.effectType)
        {
            case EffectType.DEBUFF:
            case EffectType.BUFF:
                string buffName = sourceAbility.idName + BaseEffect.statBonusType.ToString() + BaseEffect.statModifyType.ToString();
                ApplyBuffEffect(target, source, buffName);
                break;

            case EffectType.STUN:
                target.AddStatusEffect(new StunEffect(target, source, BaseEffect.effectDuration));
                break;

            default:
                break;
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