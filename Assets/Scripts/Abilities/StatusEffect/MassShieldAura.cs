using System;

public class MassShieldAura : ActorEffect
{
    public const float BASE_DAMAGE_TRANSFER = 0.5f;
    public const float BASE_DAMAGE_MITIGATION = 0.1f;
    public const float DAMAGE_MITIGATION_GROWTH = 0.001f;
    public float DamageTransferRate { get; private set; }
    public float DamageMitigation { get; private set; }

    public override GroupType StatusTag => GroupType.AFFECTED_BY_MASS_SHIELD;

    public MassShieldAura(Actor target, Actor source, float effectLevel, float duration, float auraEffectiveness) : base(target, source)
    {
        effectType = EffectType.MASS_SHIELD_AURA;
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

    public float TransferDamage(float damage)
    {
        float damageMitigationTotalMulti = DamageMitigation * Source.Data.DamageTakenModifier;
        float remainder = Source.ModifyCurrentShield(damage * damageMitigationTotalMulti, true);
        return remainder / damageMitigationTotalMulti;
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
