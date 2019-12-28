using System.Collections.Generic;
using UnityEngine;

public class BleedEffect : ActorEffect
{
    public const float BASE_DURATION = 2.0f;
    public const float BASE_DAMAGE_MULTIPLIER = 0.1f;
    public float damagePerSecond;
    protected float bonusDamageMultiplier;
    protected Vector2 lastPosition;
    protected float timeSinceLastCheck;

    public override GroupType StatusTag => GroupType.SELF_IS_BLEEDING;

    public BleedEffect(Actor target, Actor source, float inputDamage, float duration, int maxStacks = 1, float bonusMulti = 1f) : base(target, source)
    {
        effectType = EffectType.BLEED;
        damagePerSecond = inputDamage;
        this.duration = duration;
        timeSinceLastCheck = 0;
        lastPosition = target.transform.position;
        MaxStacks = source.Data.OnHitData.effectData[EffectType.BLEED].MaxStacks;
        bonusDamageMultiplier = source.Data.OnHitData.effectData[EffectType.BLEED].SecondaryEffectiveness;
    }

    public override void OnApply()
    {
        if (Source.Data.HasSpecialBonus(BonusType.BLEED_STACKS_EXPLODE_AT_MAX))
        {
            List<ActorEffect> statusList = target.GetStatusEffectAll(EffectType.BLEED);
            if (statusList.Count >= MaxStacks)
            {
                InstantEffects.ApplyStatusExplosionEffect(target, Source, EffectType.BLEED, statusList);
            }
        }
    }

    public override void OnExpire()
    {
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        float additionalDamage = 0;
        timeSinceLastCheck += tick;
        if (timeSinceLastCheck >= 0.2f && bonusDamageMultiplier > 0)
        {
            timeSinceLastCheck -= 0.2f;
            Vector2 position = target.transform.position;
            float distance = Mathf.Sqrt((position - lastPosition).sqrMagnitude);
            lastPosition = position;
            additionalDamage = distance * damagePerSecond * bonusDamageMultiplier;
        }
        target.ApplySingleElementDamage(ElementType.PHYSICAL, (damagePerSecond + additionalDamage) * tick * target.Data.OnHitData.effectData[EffectType.BLEED].Resistance, Source.Data.OnHitData, false, true);
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (1 - (target.Data.GetResistance(ElementType.PHYSICAL) - Source.Data.GetNegation(ElementType.PHYSICAL)) / 100f);
    }

    public override float GetSimpleEffectValue()
    {
        return damagePerSecond;
    }
}