using UnityEngine;

public class RadiationEffect : ActorEffect
{
    public const float BASE_DURATION = 5.0f;
    private const float DAMAGE_TICK_TIME = 1f / 3f;
    public const float BASE_DAMAGE_MULTIPLIER = 0.05f;
    protected float damagePerSecond;
    protected float timeSinceLastCheck;

    public override GroupType StatusTag => GroupType.SELF_IS_RADIATION;
    public RadiationEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.RADIATION;
        damagePerSecond = inputDamage;
        this.duration = duration;
        timeSinceLastCheck = 0;
        MaxStacks = source.Data.OnHitData.effectData[EffectType.RADIATION].MaxStacks;
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {

    }

    public override void Update(float deltaTime)
    {
        float timeElapsed = DurationUpdate(deltaTime);
        DamageTick(timeElapsed);

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


        target.ApplySingleElementDamage(ElementType.VOID, damagePerSecond * timeElapsed, Source.Data.OnHitData, false, true);

        foreach (Collider2D c in hits)
        {
            Actor actor = c.gameObject.GetComponent<Actor>();
            if (actor != null && actor != target)
                actor.ApplySingleElementDamage(ElementType.VOID, damagePerSecond * timeElapsed, Source.Data.OnHitData, false, true);
        }
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.VOID) - Source.Data.GetNegation(ElementType.VOID)) / 100f;
    }

    public override float GetSimpleEffectValue()
    {
        return damagePerSecond;
    }
}
