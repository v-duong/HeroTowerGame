using System;
using System.Collections.Generic;
using UnityEngine;

public class BerserkEffect : ActorEffect
{
    public const float BASE_DURATION = 8.0f;
    public const float BASE_COOLDOWN = 30f;
    public int effectLevel;
    public float effectCooldown;

    private List<Tuple<BonusType, ModifyType, float>> bonuses;

    public override bool StacksIncrementExistingEffect => true;


    public override GroupType StatusTag => GroupType.NO_GROUP;

    public BerserkEffect(Actor target, Actor source, float effectLevel, float duration) : base(target, source)
    {
        MaxStacks = 100;
        this.effectLevel = (int)effectLevel;
        effectType = EffectType.BERSERK;
        this.duration = duration;
        effectCooldown = 0;

        bonuses = new List<Tuple<BonusType, ModifyType, float>>
            {
                new Tuple<BonusType, ModifyType, float>(BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY, 50f + 1f * effectLevel),
                new Tuple<BonusType, ModifyType, float>(BonusType.GLOBAL_ATTACK_SPEED, ModifyType.MULTIPLY, 15f + 0.3f * effectLevel),
                new Tuple<BonusType, ModifyType, float>(BonusType.MOVEMENT_SPEED, ModifyType.ADDITIVE, 25f + 0.5f * effectLevel),
                new Tuple<BonusType, ModifyType, float>(BonusType.DAMAGE_TAKEN, ModifyType.ADDITIVE, 20f)
            };
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {
        Debug.Log("BERSERKER EXPIRE");
    }

    public override void Update(float deltaTime)
    {
        DurationUpdate(deltaTime);
        if (effectCooldown > 0)
        {
            effectCooldown -= deltaTime;
        }
    }

    public override void SetStacks(int newStackValue)
    {
        if (effectCooldown > 0)
        {
            Stacks = 1;
            return;
        }

        base.SetStacks(newStackValue);

        if (Stacks == MaxStacks)
        {
            Stacks = 1;
            effectCooldown = BASE_COOLDOWN;
            duration = BASE_COOLDOWN;
            target.AddStatusEffect(new StatBonusBuffEffect(target, Source, bonuses, 8f, "BERSERKER_BUFF", EffectType.BUFF));
        }
    }

    public override float GetEffectValue()
    {
        return effectLevel;
    }

    public override float GetSimpleEffectValue()
    {
        return effectLevel;
    }
}
