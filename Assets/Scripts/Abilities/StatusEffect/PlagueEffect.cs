using System.Collections.Generic;
using UnityEngine;

public class PlagueEffect : ActorEffect
{
    public const float BASE_DURATION = 2.0f;
    protected float damagePerSecond;
    protected float timeSinceLastCheck;

    public override GroupType StatusTag => GroupType.NO_GROUP;
    public PlagueEffect(Actor target, Actor source, float duration) : base(target, source)
    {
        effectType = EffectType.PLAGUE;
        this.duration = duration;
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {
        if (target.Data.IsDead)
        {
            float durationMod = Source.Data.HasSpecialBonus(BonusType.PLAGUE_SPREAD_DURATION_INCREASE) ? 2f : 0f;
            LayerMask mask = target.GetActorType() == ActorType.ALLY ? (LayerMask)LayerMask.GetMask("Hero") : (LayerMask)LayerMask.GetMask("Enemy");
            List<ActorEffect> effectsToApply = new List<ActorEffect>
            {
                target.GetStatusEffect(EffectType.BLEED),
                target.GetStatusEffect(EffectType.BURN),
                target.GetStatusEffect(EffectType.ELECTROCUTE),
                target.GetStatusEffect(EffectType.RADIATION),
                target.GetStatusEffect(EffectType.POISON)
            };

            Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, 1f, mask);

            foreach (Collider2D hit in hits)
            {
                Actor secondary = hit.gameObject.GetComponent<Actor>();

                if (secondary.Data.IsDead || secondary == target)
                    continue;

                foreach (ActorEffect effect in effectsToApply)
                {
                    if (effect != null)
                        ApplyEffectToTarget(secondary, effect.Source, effect.effectType, effect.GetSimpleEffectValue(), effect.duration + durationMod);
                }

                ApplyEffectToTarget(secondary, Source, EffectType.PLAGUE, 0, 8);
            }
        }
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        target.ApplySingleElementDamage(ElementType.PHYSICAL, target.Data.CurrentHealth * 0.02f * tick, Source.Data.OnHitData, false, true);
    }

    public override float GetEffectValue()
    {
        return 1;
    }

    public override float GetSimpleEffectValue()
    {
        return 1;
    }
}
