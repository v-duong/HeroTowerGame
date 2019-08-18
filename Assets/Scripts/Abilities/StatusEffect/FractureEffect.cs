using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureEffect : ActorStatusEffect
{
    protected int physReduction;
    protected int elementalReduction;

    public FractureEffect(Actor target, double effectiveness, float duration) : base(target)
    {
        effectType = EffectType.FRACTURE;
        physReduction = (int)(-10 * effectiveness);
        elementalReduction = (int)(-5 * effectiveness);
        this.duration = duration;
    }

    protected override void OnApply()
    {
        target.Data.AddTemporaryBonus(physReduction, BonusType.PHYSICAL_RESISTANCE, ModifyType.FLAT_ADDITION);
        target.Data.AddTemporaryBonus(elementalReduction, BonusType.ELEMENTAL_RESISTANCES, ModifyType.FLAT_ADDITION);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(physReduction, BonusType.PHYSICAL_RESISTANCE, ModifyType.FLAT_ADDITION);
        target.Data.RemoveTemporaryBonus(elementalReduction, BonusType.ELEMENTAL_RESISTANCES, ModifyType.FLAT_ADDITION);
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        if (duration <= 0)
            OnExpire();
    }
}
