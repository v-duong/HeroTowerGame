using System;
using UnityEngine;

public class ChillEffect : ActorEffect
{
    public const float BASE_DURATION = 2.0f;
    public const int CHILL_EFFECT_CAP = 50;
    public const float BASE_CHILL_EFFECT = -10F;
    public const float BASE_CHILL_THRESHOLD = 0.1F;
    protected int effectPower;

    public override GroupType StatusTag => GroupType.SELF_IS_CHILLED;

    public ChillEffect(Actor target, Actor source, float effectiveness, float duration) : base(target, source)
    {
        effectType = EffectType.CHILL;
        effectPower = (int)effectiveness;
        effectPower = Math.Max(effectPower, -CHILL_EFFECT_CAP);
        effectPower = Math.Min(effectPower, CHILL_EFFECT_CAP);
        this.duration = duration;
    }

    public override void OnApply()
    {
        target.Data.AddTemporaryBonus(effectPower, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY, true);
        target.Data.AddTemporaryBonus(effectPower, BonusType.SHIELD_REGEN, ModifyType.MULTIPLY, true);
        target.Data.AddTemporaryBonus(effectPower, BonusType.HEALTH_REGEN, ModifyType.MULTIPLY, true);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY, true);
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.SHIELD_REGEN, ModifyType.MULTIPLY, true);
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.HEALTH_REGEN, ModifyType.MULTIPLY, true);
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);
    }

    public override float GetEffectValue()
    {
        return effectPower;
    }

    public override float GetSimpleEffectValue()
    {
        return effectPower;
    }
}