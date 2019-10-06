using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureEffect : ActorEffect
{
    public const float BASE_DURATION = 3.0f;
    public const int FRACTURE_EFFECT_CAP = 25;
    public const float BASE_FRACTURE_EFFECT = -5F;
    public const float BASE_FRACTURE_THRESHOLD = 0.1F;
    protected int resistanceReduction;

    public override GroupType StatusTag => GroupType.SELF_IS_FRACTURED;

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
        target.Data.AddTemporaryBonus(resistanceReduction, BonusType.PHYSICAL_RESISTANCE, ModifyType.FLAT_ADDITION, true);
        target.Data.AddTemporaryBonus(resistanceReduction, BonusType.ELEMENTAL_RESISTANCES, ModifyType.FLAT_ADDITION, true);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(resistanceReduction, BonusType.PHYSICAL_RESISTANCE, ModifyType.FLAT_ADDITION, true);
        target.Data.RemoveTemporaryBonus(resistanceReduction, BonusType.ELEMENTAL_RESISTANCES, ModifyType.FLAT_ADDITION, true);
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);

    }

    public override float GetEffectValue()
    {
        return resistanceReduction;
    }

    public override float GetSimpleEffectValue()
    {
        return resistanceReduction;
    }
}
