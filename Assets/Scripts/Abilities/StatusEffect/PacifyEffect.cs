using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacifyEffect : ActorStatusEffect
{
    protected int effectPower;

    public PacifyEffect(Actor target, Actor source, float effectiveness, float duration) : base(target, source)
    {
        effectType = EffectType.PACIFY;
        effectPower = (int)(effectiveness * -15f);
        this.duration = duration;
    }

    protected override void OnApply()
    {
        target.Data.AddTemporaryBonus(effectPower, BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY);
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
