using System.Collections.Generic;
using UnityEngine;

public class ElectrocuteEffect : ActorEffect
{
    public const float BASE_DURATION = 2.0f;
    private const float BASE_RADIUS = 2f;
    public const float BASE_DAMAGE_MULTIPLIER = 0.05f;
    protected float damage;
    protected float timeElapsed;

    public override GroupType StatusTag => GroupType.SELF_IS_ELECTROCUTED;

    public ElectrocuteEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.ELECTROCUTE;
        damage = inputDamage;
        this.duration = duration;
        MaxStacks = source.Data.OnHitData.effectData[EffectType.ELECTROCUTE].MaxStacks;
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {
        if (timeElapsed == 0)
            return;

        Collider2D[] hits;

        //HashSet<GroupType> tags = new HashSet<GroupType>(Source.Data.GroupTypes);
        //tags.UnionWith(Source.GetActorTags());
        //float range = Source.Data.GetMultiStatBonus(tags, BonusType.AREA_RADIUS).CalculateStat(BASE_RADIUS);

        float range = BASE_RADIUS;

        if (target.GetActorType() == ActorType.ENEMY)
        {
            hits = Physics2D.OverlapCircleAll(target.transform.position, range, LayerMask.GetMask("Enemy"));
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(target.transform.position, range, LayerMask.GetMask("Hero"));
        }

        List<Actor> collidedActors = new List<Actor>();
        foreach (Collider2D collider in hits)
        {
            Actor targetActor = collider.gameObject.GetComponent<Actor>();
            if (targetActor == null || targetActor.Data.IsDead)
                continue;
            collidedActors.Add(targetActor);
        }

        if (collidedActors.Count > 0)
        {
            int index = Random.Range(0, collidedActors.Count);
            Actor secondaryTarget = collidedActors[index];

            if (secondaryTarget != null)
                secondaryTarget.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed, Source.Data.OnHitData, false, true);
        }

        target.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed * 1.5f, Source.Data.OnHitData, false, true);

        /*
        foreach(Collider2D c in hits)
        {
            Actor secondaryTarget = c.gameObject.GetComponent<Actor>();
            secondaryTarget.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed, false);
        }
        */
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        timeElapsed += tick;
    }

    public override float GetEffectValue()
    {
        return damage * (target.Data.GetResistance(ElementType.LIGHTNING) - Source.Data.GetNegation(ElementType.LIGHTNING)) / 100f;
    }

    public override float GetSimpleEffectValue()
    {
        return damage;
    }
}