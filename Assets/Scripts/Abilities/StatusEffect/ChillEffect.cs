using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChillEffect : ActorStatusEffect
{
    protected int effectPower;

    public ChillEffect (Actor target, double effectiveness, float duration) : base(target)
    {
        effectType = EffectType.CHILL;
        effectPower = (int)(-10 * effectiveness);
        this.duration = duration;
    }

    protected override void OnApply()
    {
        target.Data.AddTemporaryBonus(effectPower, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY);
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        if (duration <= 0)
            OnExpire();
    }
}
