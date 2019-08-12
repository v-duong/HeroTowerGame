﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorStatusEffect
{
    protected readonly Actor target;

    public EffectType effectType;
    public float duration;

    protected abstract void OnApply();
    public abstract void OnExpire();
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

    public void RefreshDuration(float duration)
    {
        duration = this.duration;
    }

    public ActorStatusEffect(Actor target)
    {
        this.target = target;
        OnApply();
    }
}

