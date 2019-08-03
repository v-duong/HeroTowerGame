using UnityEngine;

public class ElectrocuteEffect : ActorStatusEffect
{
    protected float damage;
    protected float timeElapsed;

    public ElectrocuteEffect(Actor target, double inputDamage, float duration) : base(target)
    {
        this.damage = (float)(inputDamage * 0.25d);
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

        int index = Random.Range(0, hits.Length - 1);
        Actor secondaryTarget = hits[index].gameObject.GetComponent<Actor>();
        
        target.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed, false);
        secondaryTarget.ApplySingleElementDamage(ElementType.LIGHTNING, damage * timeElapsed, false);

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