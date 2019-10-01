using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBonusBuffEffect : ActorStatusEffect
{
    public float BuffPower { get; protected set; }
    public string BuffName { get; protected set; }

    public GroupType buffType;

    public override GroupType StatusTag => buffType;

    public override int MaxStacks => 1;

    protected List<Tuple<BonusType, ModifyType, float>> bonus;

    public StatBonusBuffEffect(Actor target, Actor source, List<Tuple<BonusType,ModifyType,float>> bonuses, float duration, string buffName, EffectType effectType) : base(target, source)
    {
        this.effectType = effectType;
        this.duration = duration;
        BuffName = buffName;
        bonus = bonuses;
        BuffPower = 0;
        foreach(Tuple<BonusType, ModifyType, float> tuple in bonus)
        {
            target.Data.AddTemporaryBonus(tuple.Item3, tuple.Item1, tuple.Item2, true);
            BuffPower += tuple.Item3;
        }
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {
        foreach (Tuple<BonusType, ModifyType, float> tuple in bonus)
        {
            target.Data.RemoveTemporaryBonus(tuple.Item3, tuple.Item1, tuple.Item2, true);
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
}
