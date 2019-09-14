﻿using UnityEngine;

public class RadiationEffect : ActorStatusEffect
{
    private const float DAMAGE_TICK_TIME = 1f / 3f;
    protected float damagePerSecond;
    protected float timeSinceLastCheck;

    public override GroupType StatusTag => GroupType.SELF_IS_RADIATION;

    public RadiationEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.RADIATION;
        damagePerSecond = inputDamage / 24f;
        this.duration = duration;
        timeSinceLastCheck = 0;
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
        timeSinceLastCheck += tick;
        if (timeSinceLastCheck >= DAMAGE_TICK_TIME)
        {
            timeSinceLastCheck -= DAMAGE_TICK_TIME;
            DamageTick(tick);
        }
        if (duration <= 0)
            OnExpire();
    }

    private void DamageTick(float tick)
    {
        Collider2D[] hits;

        if (target.GetActorType() == ActorType.ENEMY)
        {
            hits = Physics2D.OverlapCircleAll(target.transform.position, 1.5f, LayerMask.GetMask("Enemy"));
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(target.transform.position, 1.5f, LayerMask.GetMask("Hero"));
        }

        int index = Random.Range(0, hits.Length);

        foreach (Collider2D c in hits)
        {
            Actor actor = c.gameObject.GetComponent<Actor>();
            actor.ApplySingleElementDamage(ElementType.VOID, damagePerSecond, Source.Data.VoidNegation, false);
        }
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.VOID) - Source.Data.VoidNegation) / 100f;
    }
}