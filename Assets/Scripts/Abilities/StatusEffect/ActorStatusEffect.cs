using System.Collections.Generic;

public abstract class ActorStatusEffect
{
    protected readonly Actor target;
    public Actor Source { get; protected set; }

    public EffectType effectType;
    public float duration;

    public abstract void OnApply();

    public abstract void OnExpire();

    public abstract void Update(float deltaTime);

    public abstract float GetEffectValue();

    public abstract GroupType StatusTag { get; }
    public abstract int MaxStacks { get; }

    protected float DurationUpdate(float dT)
    {
        if (dT > duration)
        {
            duration = 0;
            return duration;
        }
        else
        {
            duration -= dT;
            return dT;
        }
    }

    public void RefreshDuration(float duration)
    {
        if (this.duration < duration)
            this.duration = duration;
    }

    public ActorStatusEffect(Actor target, Actor source)
    {
        this.target = target;
        Source = source;
    }

    public static void ApplyEffectToTarget(Actor target, Actor source, EffectType effectType, float effectPower, float duration)
    {

    }
}