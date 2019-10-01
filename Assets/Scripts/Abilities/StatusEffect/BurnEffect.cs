public class BurnEffect : ActorStatusEffect
{
    public const float BASE_DURATION = 2.0f;
    protected float damagePerSecond;

    public override GroupType StatusTag => GroupType.SELF_IS_BURNING;

    public override int MaxStacks => 1;

    public BurnEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.BURN;
        damagePerSecond = inputDamage * 0.10f;
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
        target.ApplySingleElementDamage(ElementType.FIRE, damagePerSecond * tick, Source.Data.FireNegation, false);
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.FIRE) - Source.Data.FireNegation) / 100f;
    }
}