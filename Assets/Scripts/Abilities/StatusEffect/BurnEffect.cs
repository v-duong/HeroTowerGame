using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : ActorStatusEffect
{
    protected float damagePerSecond;

    public override GroupType StatusTag => GroupType.SELF_IS_BURNING;

    public BurnEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.BURN;
        damagePerSecond = inputDamage * 0.15f;
        this.duration = duration;

    }

    public override void OnApply()
    {
        
    }

    public override void OnExpire()
    {
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        target.ApplySingleElementDamage(ElementType.FIRE, damagePerSecond * tick, Source.Data.FireNegation, false);
        if (duration <= 0)
            OnExpire();
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.FIRE) - Source.Data.FireNegation) / 100f;
    }
}
