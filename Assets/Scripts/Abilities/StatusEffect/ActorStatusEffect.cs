using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorStatusEffect
{
    protected readonly Actor target;

    public float duration;

    protected abstract void OnApply();
    protected abstract void OnExpire();
    public abstract void Update(float deltaTime);

    protected float DurationUpdate(float dT)
    {
        if (dT > duration)
        {
            duration = 0;
            return duration;
        } else
        {
            duration -= dT;
            return dT;
        }
    }

    public ActorStatusEffect(Actor target)
    {
        this.target = target;
        OnApply();
    }
}

