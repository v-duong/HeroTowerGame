using System;
using UnityEngine;

public class ChillEffect : ActorStatusEffect
{
    public const int CHILL_EFFECT_CAP = 50;
    public const float BASE_CHILL_EFFECT = -10F;
    public const float BASE_CHILL_THRESHOLD = 0.1F;
    protected int effectPower;

    public ChillEffect(Actor target, Actor source, float effectiveness, float duration) : base(target, source)
    {
        effectType = EffectType.CHILL;
        effectPower = (int)effectiveness;
        Debug.Log(effectPower);
        effectPower = Math.Max(effectPower, -CHILL_EFFECT_CAP);
        effectPower = Math.Min(effectPower, CHILL_EFFECT_CAP);
        this.duration = duration;
    }

    public override void OnApply()
    {
        target.Data.AddTemporaryBonus(effectPower, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY);
        target.Data.AddTemporaryBonus(effectPower, BonusType.SHIELD_REGEN, ModifyType.MULTIPLY);
        target.Data.AddTemporaryBonus(effectPower, BonusType.HEALTH_REGEN, ModifyType.MULTIPLY);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY);
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.SHIELD_REGEN, ModifyType.MULTIPLY);
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.HEALTH_REGEN, ModifyType.MULTIPLY);
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        if (duration <= 0)
            OnExpire();
    }

    public override float GetEffectValue()
    {
        return effectPower;
    }
}