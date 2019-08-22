using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : ActorStatusEffect
{
    protected float damagePerSecond;

    public BurnEffect(Actor target, Actor source, float inputDamage, float duration) : base(target)
    {
        effectType = EffectType.BURNING;
        damagePerSecond = inputDamage * 0.5f;
        this.duration = duration;
    }

    protected override void OnApply()
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
}
