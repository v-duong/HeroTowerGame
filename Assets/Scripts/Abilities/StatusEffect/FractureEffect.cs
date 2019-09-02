using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureEffect : ActorStatusEffect
{
    public const int FRACTURE_EFFECT_CAP = 25;
    public const float BASE_FRACTURE_EFFECT = -5F;
    public const float BASE_FRACTURE_THRESHOLD = 0.1F;
    protected int resistanceReduction;

    public FractureEffect(Actor target, Actor source, float effectiveness, float duration) : base(target, source)
    {
        effectType = EffectType.FRACTURE;
        resistanceReduction = (int)effectiveness;
        resistanceReduction = Math.Max(resistanceReduction, -FRACTURE_EFFECT_CAP);
        resistanceReduction = Math.Min(resistanceReduction, FRACTURE_EFFECT_CAP);
        this.duration = duration;
    }

    public override void OnApply()
    {
        target.Data.AddTemporaryBonus(resistanceReduction, BonusType.PHYSICAL_RESISTANCE, ModifyType.FLAT_ADDITION);
        target.Data.AddTemporaryBonus(resistanceReduction, BonusType.ELEMENTAL_RESISTANCES, ModifyType.FLAT_ADDITION);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(resistanceReduction, BonusType.PHYSICAL_RESISTANCE, ModifyType.FLAT_ADDITION);
        target.Data.RemoveTemporaryBonus(resistanceReduction, BonusType.ELEMENTAL_RESISTANCES, ModifyType.FLAT_ADDITION);
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
        return resistanceReduction;
    }
}
