using UnityEngine;

public class RadiationEffect : ActorStatusEffect
{
    private const float DAMAGE_TICK_TIME = 1f / 3f;
    protected float damagePerSecond;
    protected float timeSinceLastCheck;

    public override GroupType StatusTag => GroupType.SELF_IS_RADIATION;

    public RadiationEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.RADIATION;
        damagePerSecond = inputDamage * 0.05f;
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
        float timeElapsed = DurationUpdate(deltaTime);
        DamageTick(timeElapsed);
        if (duration <= 0)
            OnExpire();
    }

    private void DamageTick(float timeElapsed)
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

        foreach (Collider2D c in hits)
        {
            Actor actor = c.gameObject.GetComponent<Actor>();
            actor.ApplySingleElementDamage(ElementType.VOID, damagePerSecond * timeElapsed, Source.Data.VoidNegation, false);
        }
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.VOID) - Source.Data.VoidNegation) / 100f;
    }
}