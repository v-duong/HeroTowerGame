using System.Collections.Generic;

public class StatBonusBuffEffect : SourcedActorBuffEffect
{
    public GroupType buffType;

    public override GroupType StatusTag => buffType;

    protected List<TempEffectBonusContainer.StatusBonus> bonus;

    public StatBonusBuffEffect(Actor target, Actor source, List<TempEffectBonusContainer.StatusBonus> bonuses, float duration, string buffName, EffectType effectType) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        BuffName = buffName;
        bonus = bonuses;
        BuffPower = 0;
        foreach (var tuple in bonus)
        {
            BuffPower += tuple.effectValue;
        }
    }

    public StatBonusBuffEffect(Actor target, Actor source, TempEffectBonusContainer.StatusBonus bonus, float duration, string buffName, EffectType effectType) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        BuffName = buffName;
        this.bonus = new List<TempEffectBonusContainer.StatusBonus>
        {
            bonus
        };
        BuffPower = bonus.effectValue;
    }

    public override void OnApply()
    {
        foreach (var tuple in bonus)
        {
            target.Data.AddTemporaryBonus(tuple.effectValue, tuple.bonusType, tuple.modifyType, true);
        }
    }

    public override void OnExpire()
    {
        foreach (var tuple in bonus)
        {
            target.Data.RemoveTemporaryBonus(tuple.effectValue, tuple.bonusType, tuple.modifyType, true);
        }
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
