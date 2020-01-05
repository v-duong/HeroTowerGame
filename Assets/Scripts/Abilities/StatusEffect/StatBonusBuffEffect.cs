using System.Collections.Generic;

public class StatBonusBuffEffect : SourcedActorEffect
{
    public GroupType buffType;

    public override GroupType StatusTag => buffType;

    protected List<TempEffectBonusContainer.StatusBonus> bonus;
    protected float effectMultiplier;

    public StatBonusBuffEffect(Actor target, Actor source, List<TempEffectBonusContainer.StatusBonus> bonuses, float duration, string buffName, EffectType effectType, float multiplier) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        EffectName = buffName;
        bonus = bonuses;
        EffectPower = 0;
        effectMultiplier = multiplier;
        foreach (var tuple in bonus)
        {
            EffectPower += tuple.effectValue * multiplier;
        }
    }

    public StatBonusBuffEffect(Actor target, Actor source, TempEffectBonusContainer.StatusBonus bonus, float duration, string buffName, EffectType effectType, float multiplier) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        EffectName = buffName;
        effectMultiplier = multiplier;
        this.bonus = new List<TempEffectBonusContainer.StatusBonus>
        {
            bonus
        };
        EffectPower = bonus.effectValue * multiplier;
    }

    public override void OnApply()
    {
        foreach (var tuple in bonus)
        {
            target.Data.AddTemporaryBonus(tuple.effectValue * effectMultiplier, tuple.bonusType, tuple.modifyType, true);
        }
    }

    public override void OnExpire()
    {
        foreach (var tuple in bonus)
        {
            target.Data.RemoveTemporaryBonus(tuple.effectValue * effectMultiplier, tuple.bonusType, tuple.modifyType, true);
        }
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
