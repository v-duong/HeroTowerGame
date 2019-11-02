using System;
using System.Collections.Generic;

public class RepeatOffenderBuffEffect : ActorEffect
{
    public const float BASE_DURATION = 5.0f;
    public int effectLevel;
    public float effectCooldown;

    private List<Tuple<BonusType, ModifyType, float>> bonuses;

    public override GroupType StatusTag => GroupType.NO_GROUP;

    public RepeatOffenderBuffEffect(Actor target, Actor source, float effectLevel, float duration) : base(target, source)
    {
        MaxStacks = target.Data.GetMultiStatBonus(target.GetActorTagsAndDataTags(), BonusType.REPEAT_OFFENDER_MAX_STACKS).CalculateStat(5);
        this.effectLevel = (int)effectLevel;
        effectType = EffectType.REPEAT_OFFENDER_BUFF;
        this.duration = duration;
        effectCooldown = 0;

        // 100% crit chance, 150 crit damage
        bonuses = new List<Tuple<BonusType, ModifyType, float>>
            {
                new Tuple<BonusType, ModifyType, float>(BonusType.GLOBAL_CRITICAL_CHANCE, ModifyType.ADDITIVE, 5f + 0.3f * effectLevel),
                new Tuple<BonusType, ModifyType, float>(BonusType.GLOBAL_CRITICAL_DAMAGE, ModifyType.FLAT_ADDITION, 10f + 0.4f * effectLevel),
            };

        if (target.Data.HasSpecialBonus(BonusType.REPEAT_OFFENDER_GIVES_ATTACK_SPEED))
        {
            bonuses.Add(new Tuple<BonusType, ModifyType, float>(BonusType.GLOBAL_ATTACK_SPEED, ModifyType.ADDITIVE, 0f + 0.1f * effectLevel));
            bonuses.Add(new Tuple<BonusType, ModifyType, float>(BonusType.CAST_SPEED, ModifyType.ADDITIVE, 0f + 0.1f * effectLevel));
        }

        if (target.Data.HasSpecialBonus(BonusType.REPEAT_OFFENDER_GIVES_FLAT_CRIT))
        {
            bonuses.Add(new Tuple<BonusType, ModifyType, float>(BonusType.GLOBAL_CRITICAL_CHANCE, ModifyType.FLAT_ADDITION, 0.4f));
        }
    }

    public override void OnApply()
    {
        foreach (Tuple<BonusType, ModifyType, float> tuple in bonuses)
        {
            target.Data.AddTemporaryBonus(tuple.Item3, tuple.Item1, tuple.Item2, true);
        }
    }

    public override void OnExpire()
    {
        foreach (Tuple<BonusType, ModifyType, float> tuple in bonuses)
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
        return effectLevel;
    }

    public override float GetSimpleEffectValue()
    {
        return effectLevel;
    }
}