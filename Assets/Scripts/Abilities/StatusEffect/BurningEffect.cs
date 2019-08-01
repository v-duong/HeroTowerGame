using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningEffect : ActorStatusEffect
{
    protected float damagePerSecond;

    public BurningEffect(Actor target, double inputDamage, float duration) : base(target)
    {
        damagePerSecond = (float)(inputDamage * 0.5d);
        this.duration = duration;
    }

    protected override void OnApply()
    {
        
    }

    protected override void OnExpire()
    {
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        target.ApplySingleElementDamage(ElementType.FIRE, damagePerSecond * tick, false);
        if (duration <= 0)
            OnExpire();
    }
}
