using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBonusBuffEffect : ActorStatusEffect
{
    public float BuffPower { get; protected set; }
    public string BuffName { get; protected set; }
    protected List<Tuple<BonusType, ModifyType, int>> bonus;

    public StatBonusBuffEffect(Actor target, List<Tuple<BonusType,ModifyType,int>> bonuses, float duration, string buffName, EffectType effectType) : base(target)
    {
        this.effectType = effectType;
        this.duration = duration;
        this.BuffName = buffName;
        bonus = bonuses;
        foreach(Tuple<BonusType, ModifyType, int> tuple in bonus)
        {
            target.Data.AddTemporaryBonus(tuple.Item3, tuple.Item1, tuple.Item2);
        }
    }

    protected override void OnApply()
    {
    }

    public override void OnExpire()
    {
        foreach (Tuple<BonusType, ModifyType, int> tuple in bonus)
        {
            target.Data.RemoveTemporaryBonus(tuple.Item3, tuple.Item1, tuple.Item2);
        }
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        if (duration <= 0)
            OnExpire();
    }

}
