using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorStatusEffect
{
    readonly Actor target;
    readonly Actor effectSource;

    public abstract void Apply();
    public abstract void Remove();
    public abstract void Update();

    public ActorStatusEffect(Actor target, Actor source)
    {
        this.target = target;
        effectSource = source;
    }
}

