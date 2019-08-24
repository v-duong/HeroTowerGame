using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChillEffect : ActorStatusEffect
{
    protected int effectPower;

    public ChillEffect (Actor target, Actor source, float effectiveness, float duration) : base(target, source)
    {
        effectType = EffectType.CHILL;
        effectPower = (int)(-10f * effectiveness);
        this.duration = duration;
    }

    protected override void OnApply()
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
