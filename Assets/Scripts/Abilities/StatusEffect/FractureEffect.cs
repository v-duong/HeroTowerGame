using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureEffect : ActorStatusEffect
{
    protected int effectPower;

    public FractureEffect(Actor target, double effectiveness, float duration) : base(target)
    {
        effectType = EffectType.FRACTURE;
        effectPower = (int)(-20 * 0.5d);
        this.duration = duration;
    }

    protected override void OnApply()
    {
        target.Data.AddTemporaryBonus(effectPower, BonusType.GLOBAL_ARMOR, ModifyType.MULTIPLY);
        target.Data.AddTemporaryBonus(effectPower, BonusType.GLOBAL_MAX_SHIELD, ModifyType.MULTIPLY);
        target.Data.AddTemporaryBonus(effectPower, BonusType.GLOBAL_DODGE_RATING, ModifyType.MULTIPLY);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.GLOBAL_ARMOR, ModifyType.MULTIPLY);
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.GLOBAL_MAX_SHIELD, ModifyType.MULTIPLY);
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.GLOBAL_DODGE_RATING, ModifyType.MULTIPLY);
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        if (duration <= 0)
            OnExpire();
    }
}
