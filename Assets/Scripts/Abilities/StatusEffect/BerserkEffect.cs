using System;
using System.Collections.Generic;

public class BerserkEffect : ActorEffect
{
    public const float BASE_DURATION = 12.0f;
    public const float BASE_COOLDOWN = 30f;
    public int effectLevel;
    public float effectCooldown;

    private List<TempEffectBonusContainer.StatusBonus> bonuses;

    public override bool StacksIncrementExistingEffect => true;

    public override GroupType StatusTag => GroupType.NO_GROUP;

    public BerserkEffect(Actor target, Actor source, float effectLevel, float duration) : base(target, source)
    {
        MaxStacks = 100;
        this.effectLevel = (int)effectLevel;
        effectType = EffectType.BERSERK;
        this.duration = duration;
        effectCooldown = 0;

        // final berserker buff
        // 1.75x global damage
        // 1.35x attack speed
        // 1.50x movement speed
        // +20% damage taken
        bonuses = new List<TempEffectBonusContainer.StatusBonus>
            {
                new TempEffectBonusContainer.StatusBonus(BonusType.GLOBAL_DAMAGE, ModifyType.MULTIPLY, 75f, 12f),
                new TempEffectBonusContainer.StatusBonus(BonusType.GLOBAL_ATTACK_SPEED, ModifyType.MULTIPLY, 35f, 12f),
                new TempEffectBonusContainer.StatusBonus(BonusType.MOVEMENT_SPEED, ModifyType.ADDITIVE, 50f, 12f),
                new TempEffectBonusContainer.StatusBonus(BonusType.DAMAGE_TAKEN, ModifyType.ADDITIVE, 20f, 12f)
            };
    }

    public override void OnApply()
    {
    }

    public override void OnExpire()
    {
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
        duration = BASE_DURATION;

        if (Stacks == MaxStacks)
        {
            Stacks = 1;
            effectCooldown = BASE_COOLDOWN;
            duration = BASE_COOLDOWN;
            target.AddStatusEffect(new StatBonusBuffEffect(target, Source, bonuses, 12f, "BERSERKER_BUFF", EffectType.BUFF, 1f));
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