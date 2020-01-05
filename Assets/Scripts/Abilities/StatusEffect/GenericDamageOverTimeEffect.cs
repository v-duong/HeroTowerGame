public class GenericDamageOverTimeEffect : SourcedActorEffect
{
    public float damagePerSecond;
    public ElementType element;

    public override GroupType StatusTag => GroupType.NO_GROUP;

    public GenericDamageOverTimeEffect(Actor target, Actor source, float inputDamage, float duration, ElementType elementType) : base(target, source)
    {
        effectType = EffectType.GENERIC_DAMAGE_OVER_TIME;
        damagePerSecond = inputDamage;
        this.duration = duration;
        element = elementType;
        MaxStacks = 1;
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
        target.ApplySingleElementDamage(element, damagePerSecond * tick, Source.Data.OnHitData, false, true);
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (1 - target.Data.GetResistance(element));
    }

    public override float GetSimpleEffectValue()
    {
        return damagePerSecond;
    }
}