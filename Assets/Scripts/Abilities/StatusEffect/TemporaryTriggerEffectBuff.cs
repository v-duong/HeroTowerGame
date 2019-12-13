using System.Collections.Generic;

public class TemporaryTriggerEffectBuff : SourcedActorBuffEffect
{

    public GroupType buffType;

    public override GroupType StatusTag => buffType;

    protected TriggeredEffectBonusProperty bonus;

    public TemporaryTriggerEffectBuff(Actor target, Actor source, TriggeredEffectBonusProperty bonus, float value, float duration, string buffName, EffectType effectType) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        BuffName = buffName;
        this.bonus = bonus;
        BuffPower = value;
    }

    public override void OnApply()
    {
        TriggeredEffect t = new TriggeredEffect(bonus, BuffPower, BuffName);
        target.Data.AddTriggeredEffect(bonus, t);
    }

    public override void OnExpire()
    {
        target.Data.RemoveTriggeredEffect(bonus);
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);
    }

    public override float GetEffectValue()
    {
        return BuffPower;
    }

    public override float GetSimpleEffectValue()
    {
        return BuffPower;
    }
}