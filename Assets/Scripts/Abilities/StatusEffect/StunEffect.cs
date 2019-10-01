public class StunEffect : ActorStatusEffect
{
    public override GroupType StatusTag => GroupType.NO_GROUP;

    public override int MaxStacks => 1;

    public StunEffect(Actor target, Actor source, float duration) : base(target, source)
    {
        effectType = EffectType.STUN;
        this.duration = duration;
    }

    public override void OnApply()
    {
        target.attackLocks++;
        target.Data.AddTemporaryBonus(-100, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY, true);
    }

    public override void OnExpire()
    {
        target.attackLocks--;
        target.Data.RemoveTemporaryBonus(-100, BonusType.MOVEMENT_SPEED, ModifyType.MULTIPLY, true);
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);
    }

    public override float GetEffectValue()
    {
        return 999f;
    }
}