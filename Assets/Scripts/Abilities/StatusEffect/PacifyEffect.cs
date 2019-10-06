using System;

public class PacifyEffect : ActorEffect
{
    public const float BASE_DURATION = 3.0f;
    public const int PACIFY_EFFECT_CAP = 35;
    public const float BASE_PACIFY_EFFECT = -10F;
    public const float BASE_PACIFY_THRESHOLD = 0.1F;
    protected int effectPower;

    public override GroupType StatusTag => GroupType.SELF_IS_PACIFIED;

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
        target.Data.AddTemporaryBonus(effectPower, BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY, true);
        target.Data.AddTemporaryBonus(effectPower * 0.5f, BonusType.GLOBAL_ABILITY_SPEED, ModifyType.MULTIPLY, true);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTemporaryBonus(effectPower, BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY, true);
        target.Data.AddTemporaryBonus(effectPower * 0.5f, BonusType.GLOBAL_ABILITY_SPEED, ModifyType.MULTIPLY, true);
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);
    }

    public override float GetEffectValue()
    {
        return effectPower;
    }

    public override float GetSimpleEffectValue()
    {
        return effectPower;
    }
}