using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorStatusEffect
{
    readonly Actor target;
    readonly Actor effectSource;

    public float duration;

    public abstract void OnApply();
    public abstract void OnExpire();
    public abstract void Update();

    public ActorStatusEffect(Actor target, Actor source)
    {
        this.target = target;
        this.target.AddStatusEffect(this);
        effectSource = source;
        OnApply();
    }
}

