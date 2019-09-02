using System;

public class PacifyEffect : ActorStatusEffect
{
    public const int PACIFY_EFFECT_CAP = 35;
    public const float BASE_PACIFY_EFFECT = -10F;
    public const float BASE_PACIFY_THRESHOLD = 0.1F;
    protected int effectPower;

    public PacifyEffect(Actor target, Actor source, float effectiveness, float duration) : base(target, source)
    {
        effectType = EffectType.PACIFY;
        effectPower = (int)effectiveness;
        effectPower = Math.Max(effectPower, -PACIFY_EFFECT_CAP);
        effectPower = Math.Min(effectPower, PACIFY_EFFECT_CAP);
        this.duration = duration;
    }

    public override void OnApply()
    {
        target.Data.AddTemporaryBonus(effectPower, BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY);
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        if (duration <= 0)
            OnExpire();
    }

    public override float GetEffectValue()
    {
        return effectPower;
    }
}