using System.Collections.Generic;
using UnityEngine;

public class BleedEffect : ActorEffect
{
    public const float BASE_DURATION = 2.0f;
    public const float BASE_DAMAGE_MULTIPLIER = 0.1f;
    protected float damagePerSecond;
    protected Vector2 lastPosition;
    protected float timeSinceLastCheck;

    public override GroupType StatusTag => GroupType.SELF_IS_BLEEDING;

    public BleedEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.BLEED;
        damagePerSecond = inputDamage;
        this.duration = duration;
        timeSinceLastCheck = 0;
        lastPosition = target.transform.position;
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {

    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        float additionalDamage = 0;
        timeSinceLastCheck += tick;
        if (timeSinceLastCheck >= 0.2f)
        {
            timeSinceLastCheck -= 0.2f;
            Vector2 position = target.transform.position;
            float distance = Mathf.Sqrt((position - lastPosition).sqrMagnitude);
            lastPosition = position;
            additionalDamage = distance * damagePerSecond * 0.2f;
        }
        target.ApplySingleElementDamage(ElementType.PHYSICAL, damagePerSecond * tick + additionalDamage, Source.Data.OnHitData, false, true);
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.PHYSICAL) - Source.Data.GetNegation(ElementType.PHYSICAL)) / 100f ;
    }

    public override float GetSimpleEffectValue()
    {
        return damagePerSecond;
    }
}