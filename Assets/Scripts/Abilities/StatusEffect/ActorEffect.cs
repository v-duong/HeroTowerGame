using System.Collections.Generic;
using UnityEngine;

public abstract class ActorEffect
{
    public static Collider2D[] hits = new Collider2D[50];

    protected readonly Actor target;
    public Actor Source { get; protected set; }

    public EffectType effectType;
    public float duration;

    public abstract void OnApply();

    public abstract void OnExpire();

    public abstract void Update(float deltaTime);

    public abstract float GetEffectValue();

    public abstract float GetSimpleEffectValue();

    public abstract GroupType StatusTag { get; }
    public int MaxStacks = 1;
    public virtual bool StacksIncrementExistingEffect => false;
    public int Stacks { get; protected set;}

    public virtual void SetStacks(int newStackValue)
    {
        Stacks = newStackValue;
        if (newStackValue > MaxStacks)
            Stacks = MaxStacks;
    }

    protected float DurationUpdate(float dT)
    {
        if (dT > duration)
        {
            float oldDuration = duration;
            duration = 0;
            return oldDuration;
        }
        else
        {
            duration -= dT;
            return dT;
        }
    }

    public virtual void RefreshDuration(float duration)
    {
        if (this.duration < duration)
            this.duration = duration;
    }

    public ActorEffect(Actor target, Actor source)
    {
        this.target = target;
        Source = source;
        Stacks = 1;
    }

    public static void ApplyEffectToTarget(Actor target, Actor source, EffectType effectType, float effectPower, float duration, float auraEffectiveness = 1.0f, ElementType element = ElementType.PHYSICAL)
    {
        LayerMask mask = target.GetActorType() == ActorType.ALLY ? (LayerMask)LayerMask.GetMask("Hero") : (LayerMask)LayerMask.GetMask("Enemy");
        switch (effectType)
        {
            case EffectType.BLEED:
                target.AddStatusEffect(new BleedEffect(target, source, effectPower * auraEffectiveness, duration));
                break;

            case EffectType.BURN:
                target.AddStatusEffect(new BurnEffect(target, source, effectPower * auraEffectiveness , duration));
                break;

            case EffectType.CHILL:
                target.AddStatusEffect(new ChillEffect(target, source, effectPower * auraEffectiveness, duration));
                break;

            case EffectType.ELECTROCUTE:
                target.AddStatusEffect(new ElectrocuteEffect(target, source, effectPower * auraEffectiveness, duration));
                break;

            case EffectType.FRACTURE:
                target.AddStatusEffect(new FractureEffect(target, source, effectPower * auraEffectiveness, duration));
                break;

            case EffectType.PACIFY:
                target.AddStatusEffect(new PacifyEffect(target, source, effectPower * auraEffectiveness, duration));
                break;

            case EffectType.RADIATION:
                target.AddStatusEffect(new RadiationEffect(target, source, effectPower * auraEffectiveness , duration));
                break;

            case EffectType.POISON:
                target.AddStatusEffect(new PoisonEffect(target, source, effectPower * auraEffectiveness, duration));
                break;

            case EffectType.STUN:
                target.AddStatusEffect(new StunEffect(target, source, duration));
                break;

            case EffectType.PLAGUE:
                target.AddStatusEffect(new PlagueEffect(target, source, duration));
                break;

            case EffectType.EXPLODE_MAX_LIFE:
            case EffectType.EXPLODE_OVERKILL:
            case EffectType.EXPLODE_HIT_DAMAGE:
            case EffectType.EXPLODE_BLEED:
            case EffectType.EXPLODE_BURN:
            case EffectType.EXPLODE_ELECTROCUTE:
            case EffectType.EXPLODE_RADIATION:
                source.StartCoroutine(InstantEffects.ApplyExplosionEffect(target, source, effectType, mask, effectPower, element));
                break;

            case EffectType.SPREAD_DAMAGING_STATUSES:
            case EffectType.SPREAD_BLEED:
            case EffectType.SPREAD_BURN:
            case EffectType.SPREAD_RADIATION:
            case EffectType.SPREAD_STATUSES:
                InstantEffects.ApplyStatusSpreadEffect(target, source, effectType, mask);
                break;

            case EffectType.CLEAR_STATUSES:
                break;

            case EffectType.BUFF:
                break;

            case EffectType.DEBUFF:
                break;

            case EffectType.BODYGUARD_AURA:
                target.AddStatusEffect(new BodyguardAura(target, source, effectPower, duration, auraEffectiveness));
                break;

            case EffectType.MASS_SHIELD_AURA:
                target.AddStatusEffect(new MassShieldAura(target, source, effectPower, duration, auraEffectiveness));
                break;
            case EffectType.BERSERK:
                target.AddStatusEffect(new BerserkEffect(target, source, effectPower, duration));
                break;
            case EffectType.KNOCKBACK:
                break;
            case EffectType.REPEAT_OFFENDER_BUFF:
                target.AddStatusEffect(new RepeatOffenderBuffEffect(target, source, effectPower, duration));
                break;
            case EffectType.RETALIATION_DAMAGE:
                //source.StartCoroutine(InstantEffects.ApplyRetaliationDamageEffect(target, source, mask, effectPower, element));
                break;
            default:
                return;
        }
    }

    public ActorEffect Clone()
    {
        return (ActorEffect)MemberwiseClone();
    }
}

public abstract class SourcedActorBuffEffect : ActorEffect
{
    public string BuffName;
    public float BuffPower;

    public SourcedActorBuffEffect(Actor target, Actor source) : base(target, source)
    {
    }
}