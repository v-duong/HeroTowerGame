public class PoisonEffect : ActorEffect
{
    public const int BASE_MAX_STACKS = 30;
    public const float BASE_DURATION = 3f;
    public const float BASE_DAMAGE_MULTIPLIER = 0.03f;
    protected float damagePerSecond;

    public override GroupType StatusTag => GroupType.SELF_IS_POISONED;

    public PoisonEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.POISON;
        damagePerSecond = inputDamage;
        this.duration = duration;
        MaxStacks = source.Data.OnHitData.effectData[EffectType.POISON].MaxStacks;
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
        target.ApplySingleElementDamage(ElementType.PHYSICAL, damagePerSecond * tick, Source.Data.OnHitData, false, true, EffectType.POISON);
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.PHYSICAL) - Source.Data.GetNegation(ElementType.PHYSICAL)) / 100f;
    }

    public override float GetSimpleEffectValue()
    {
        return damagePerSecond;
    }
}