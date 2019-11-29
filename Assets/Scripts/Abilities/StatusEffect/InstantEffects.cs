using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstantEffects
{
    private static readonly List<GroupType> ExplosionTags = new List<GroupType>() { GroupType.AREA };
    private static readonly List<GroupType> RetaliationDamageTags = new List<GroupType>() { GroupType.RETALIATION };

    public static IEnumerator ApplyRetaliationDamageEffect(Actor target, Actor source, Dictionary<ElementType, MinMaxRange> damageDict)
    {
        yield return null;
        target.ApplyDamage(source.ScaleSecondaryDamageValue(target, damageDict, RetaliationDamageTags), source.Data.OnHitData, true, false);
    }

    public static IEnumerator ApplyExplosionEffect(Actor target, Actor source, EffectType explosionType, LayerMask mask, float effectPower, ElementType element)
    {
        yield return null;
        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, 0.75f, mask);
        foreach (Collider2D hit in hits)
        {
            Actor secondary = hit.gameObject.GetComponent<Actor>();

            if (secondary.Data.IsDead || secondary == target)
                continue;

            Dictionary<ElementType, MinMaxRange> damageDict = new Dictionary<ElementType, MinMaxRange>
                    {
                        { element, new MinMaxRange((int)(target.Data.MaximumHealth * (effectPower / 100f)),(int)(target.Data.MaximumHealth * (effectPower / 100f))) }
                    };

            secondary.ApplyDamage(source.ScaleSecondaryDamageValue(secondary, damageDict, ExplosionTags), source.Data.OnHitData, true, true);
        }
    }

    public static void ApplyStatusExplosionEffect(Actor target, Actor source, EffectType statusToExplode, List<ActorEffect> listToExplode = null)
    {
        LayerMask mask = target.GetActorType() == ActorType.ALLY ? (LayerMask)LayerMask.GetMask("Hero") : (LayerMask)LayerMask.GetMask("Enemy");

        if (listToExplode == null)
        {
            listToExplode = target.GetStatusEffectAll(statusToExplode);
        }

        float explodeDamage = 0f;
        ElementType explodeElement = ElementType.PHYSICAL;
        EffectApplicationFlags restrictionFlags = EffectApplicationFlags.NONE;

        if (statusToExplode == EffectType.BLEED)
        {
            restrictionFlags |= EffectApplicationFlags.CANNOT_BLEED;
            explodeElement = ElementType.PHYSICAL;
            foreach (BleedEffect bleedEffect in listToExplode)
            {
                explodeDamage += bleedEffect.damagePerSecond * bleedEffect.duration * 0.5f;
                bleedEffect.duration = 0;
                target.RemoveStatusEffect(bleedEffect, true);
            }
        }

        if (explodeDamage == 0)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, 0.75f, mask);
        foreach (Collider2D hit in hits)
        {
            Actor aoeTarget = hit.gameObject.GetComponent<Actor>();

            if (aoeTarget.Data.IsDead)
                continue;

            Dictionary<ElementType, MinMaxRange> damageDict = new Dictionary<ElementType, MinMaxRange>
                    {
                        { explodeElement, new MinMaxRange((int)explodeDamage,(int)explodeDamage) }
                    };

            aoeTarget.ApplyDamage(source.ScaleSecondaryDamageValue(aoeTarget, damageDict, ExplosionTags), source.Data.OnHitData, true, true, statusToExplode, restrictionFlags);
        }
    }

    public static void ApplyStatusSpreadEffect(Actor target, Actor source, EffectType effectType, LayerMask mask)
    {
        List<ActorEffect> effectsToApply = new List<ActorEffect>();

        switch (effectType)
        {
            case EffectType.SPREAD_STATUSES:
                effectsToApply.Add(target.GetStatusEffect(EffectType.BLEED));
                effectsToApply.Add(target.GetStatusEffect(EffectType.BURN));
                effectsToApply.Add(target.GetStatusEffect(EffectType.CHILL));
                effectsToApply.Add(target.GetStatusEffect(EffectType.ELECTROCUTE));
                effectsToApply.Add(target.GetStatusEffect(EffectType.FRACTURE));
                effectsToApply.Add(target.GetStatusEffect(EffectType.PACIFY));
                effectsToApply.Add(target.GetStatusEffect(EffectType.RADIATION));
                break;

            case EffectType.SPREAD_BURN:
                effectsToApply.Add(target.GetStatusEffect(EffectType.BURN));
                break;

            case EffectType.SPREAD_DAMAGING_STATUSES:
                effectsToApply.Add(target.GetStatusEffect(EffectType.BLEED));
                effectsToApply.Add(target.GetStatusEffect(EffectType.BURN));
                effectsToApply.Add(target.GetStatusEffect(EffectType.ELECTROCUTE));
                effectsToApply.Add(target.GetStatusEffect(EffectType.RADIATION));
                break;

            case EffectType.SPREAD_BLEED:
                effectsToApply.Add(target.GetStatusEffect(EffectType.BLEED));
                break;

            case EffectType.SPREAD_RADIATION:
                effectsToApply.Add(target.GetStatusEffect(EffectType.RADIATION));
                break;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, 1f, mask);

        foreach (Collider2D hit in hits)
        {
            Actor secondary = hit.gameObject.GetComponent<Actor>();

            if (secondary.Data.IsDead || secondary == target)
                continue;

            foreach (ActorEffect effect in effectsToApply)
            {
                if (effect != null)
                    ActorEffect.ApplyEffectToTarget(secondary, effect.Source, effect.effectType, effect.GetSimpleEffectValue(), effect.duration);
            }
        }
    }
}