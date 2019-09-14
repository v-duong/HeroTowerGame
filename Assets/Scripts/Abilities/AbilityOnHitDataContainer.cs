using System;
using System.Collections.Generic;

public class AbilityOnHitDataContainer
{
    public class OnHitStatusEffectData
    {
        public float chance = 0;
        public float effectiveness = 0;
        public float duration = 0;
    }

    public class OnHitBuffEffect
    {
        public AbilityScalingAddedEffect effectBase;
        public float power = 0;

        public OnHitBuffEffect(AbilityScalingAddedEffect appliedEffect, int level)
        {
            effectBase = appliedEffect;
            power = appliedEffect.initialValue + appliedEffect.growthValue * level;
        }

        public void ApplyEffect(Actor target, Actor source, string buffName)
        {
            List<StatBonusBuffEffect> buffs = target.GetBuffStatusEffect(buffName);

            if (buffs.Count >= effectBase.stacks)
            {
                float lowestDuration = 100f;
                StatBonusBuffEffect buff = null;
                foreach (StatBonusBuffEffect b in buffs)
                {
                    if (b.duration < lowestDuration)
                        buff = b;
                }
                buff.RefreshDuration(effectBase.duration);
                return;
            }

            List<Tuple<BonusType, ModifyType, float>> bonuses = new List<Tuple<BonusType, ModifyType, float>>
            {
                new Tuple<BonusType, ModifyType, float>(effectBase.bonusType, effectBase.modifyType, power)
            };

            target.AddStatusEffect(new StatBonusBuffEffect(target, source, bonuses, effectBase.duration, buffName, effectBase.type));
        }
    }

    public string abilityName;
    public AbilityType Type;
    public Actor sourceActor;
    public List<OnHitBuffEffect> onHitEffects;
    public float accuracy;
    public int physicalNegation;
    public int fireNegation;
    public int coldNegation;
    public int lightningNegation;
    public int earthNegation;
    public int divineNegation;
    public int voidNegation;

    public float vsBossDamage;

    private readonly Dictionary<EffectType, OnHitStatusEffectData> effectData;

    public float GetEffectChance(EffectType type) => effectData[type].chance;

    public void SetEffectChance(EffectType type, float value) => effectData[type].chance = value;

    public float GetEffectEffectiveness(EffectType type) => effectData[type].effectiveness;

    public void SetEffectEffectiveness(EffectType type, float value) => effectData[type].effectiveness = value;

    public float GetEffectDuration(EffectType type) => effectData[type].duration;

    public void SetEffectDuration(EffectType type, float value) => effectData[type].duration = value;

    public OnHitStatusEffectData GetEffectData(EffectType type) => effectData[type];

    public AbilityOnHitDataContainer()
    {
        effectData = new Dictionary<EffectType, OnHitStatusEffectData>
        {
            { EffectType.BLEED, new OnHitStatusEffectData() },
            { EffectType.BURN, new OnHitStatusEffectData() },
            { EffectType.CHILL, new OnHitStatusEffectData() },
            { EffectType.ELECTROCUTE, new OnHitStatusEffectData() },
            { EffectType.FRACTURE, new OnHitStatusEffectData() },
            { EffectType.PACIFY, new OnHitStatusEffectData() },
            { EffectType.RADIATION, new OnHitStatusEffectData() }
        };
        onHitEffects = new List<OnHitBuffEffect>();
    }

    public AbilityOnHitDataContainer DeepCopy()
    {
        return (AbilityOnHitDataContainer)MemberwiseClone();
    }

    public bool DidEffectProc(EffectType effectType)
    {
        float chance = GetEffectChance(effectType);
        return Helpers.RollChance(chance / 100f);
    }
}