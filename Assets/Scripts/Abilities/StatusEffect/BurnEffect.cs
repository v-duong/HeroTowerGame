public class BurnEffect : ActorEffect
{
    public const float BASE_DURATION = 2.0f;
    public const float BASE_DAMAGE_MULTIPLIER = 0.10f;
    protected float damagePerSecond;

    public override GroupType StatusTag => GroupType.SELF_IS_BURNING;

    public BurnEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.BURN;
        damagePerSecond = inputDamage;
        this.duration = duration;
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
        target.ApplySingleElementDamage(ElementType.FIRE, damagePerSecond * tick, Source.Data.OnHitData, false, true);
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.FIRE) - Source.Data.GetNegation(ElementType.FIRE)) / 100f;
    }

    public override float GetSimpleEffectValue()
    {
        return damagePerSecond;
    }
}