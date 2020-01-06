using System;
using System.Collections.Generic;
using UnityEngine;

public class BodyguardAura : ActorEffect
{
    public const float BASE_DAMAGE_TRANSFER = 0.60f;
    public const float BASE_DAMAGE_MITIGATION = 0.00f;
    public const float DAMAGE_MITIGATION_GROWTH = 0.001f;
    public float DamageTransferRate { get; private set; }
    public float DamageMitigation { get; private set; }

    public override GroupType StatusTag => GroupType.AFFECTED_BY_BODYGUARD;

    public BodyguardAura(Actor target, Actor source, float effectLevel, float duration, float auraEffectiveness) : base(target, source)
    {
        effectType = EffectType.BODYGUARD_AURA;
        DamageTransferRate = Math.Min(BASE_DAMAGE_TRANSFER * auraEffectiveness, 1f);
        DamageMitigation = 1f - ((BASE_DAMAGE_MITIGATION + (DAMAGE_MITIGATION_GROWTH * effectLevel)) * auraEffectiveness);

        this.duration = duration;
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {
    }

    public bool TransferDamage(float damage, out float damageMod)
    {
        damageMod = DamageMitigation * Source.Data.DamageTakenModifier;
        float damageTaken = damage * damageMod;

        if (damageTaken > Source.Data.CurrentHealth || damage == 0)
            return false;

        if (Actor.DidTargetBlock(Source))
        {
            damageMod *= 1f - Source.Data.BlockProtection;
            damageTaken *= 1f - Source.Data.BlockProtection;
        }

        damageTaken = Source.ModifyCurrentShield(damageTaken, true);
        Source.ModifyCurrentHealth(damageTaken);
        return true;
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);
    }

    public override float GetEffectValue()
    {
        return DamageTransferRate;
    }

    public override float GetSimpleEffectValue()
    {
        return DamageTransferRate;
    }
}