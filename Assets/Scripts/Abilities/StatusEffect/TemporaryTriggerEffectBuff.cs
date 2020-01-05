using System.Collections.Generic;

public class TemporaryTriggerEffectBuff : SourcedActorEffect
{

    public GroupType buffType;

    public override GroupType StatusTag => buffType;

    protected TriggeredEffectBonusProperty bonus;

    public TemporaryTriggerEffectBuff(Actor target, Actor source, TriggeredEffectBonusProperty bonus, float value, float duration, string buffName, EffectType effectType) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        EffectName = buffName;
        this.bonus = bonus;
        EffectPower = value;
    }

    public override void OnApply()
    {
        TriggeredEffect t = new TriggeredEffect(bonus, EffectPower, EffectName);
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
        return EffectPower;
    }

    public override float GetSimpleEffectValue()
    {
        return EffectPower;
    }
}