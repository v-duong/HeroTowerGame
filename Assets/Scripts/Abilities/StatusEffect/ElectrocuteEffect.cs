﻿using UnityEngine;

public class ElectrocuteEffect : ActorStatusEffect
{
    protected float damage;
    protected float timeElapsed;

    public ElectrocuteEffect(Actor target, Actor source, float inputDamage, float duration) : base(target)
    {
        effectType = EffectType.ELECTROCUTE;
        this.damage = inputDamage * 0.25f;
        this.duration = duration;
    }

    protected override void OnApply()
    {
    }

    public override void OnExpire()
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
        Actor secondaryTarget = hits[index].gameObject.GetComponent<Actor>();
        
        target.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed, Source.Data.LightningNegation, false);
        secondaryTarget.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed,Source.Data.LightningNegation, false);

        /*
        foreach(Collider2D c in hits)
        {
            Actor secondaryTarget = c.gameObject.GetComponent<Actor>();
            secondaryTarget.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed, false);
        }
        */


        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        timeElapsed += tick;
        if (duration <= 0)
            OnExpire();
    }
}