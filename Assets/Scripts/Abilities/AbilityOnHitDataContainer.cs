using System;
using System.Collections.Generic;

public class AbilityOnHitDataContainer
{

    public AbilityBase sourceAbility;
    public AbilityType Type;
    public Actor sourceActor;
    public List<TriggeredEffect> onHitEffectsFromAbility;
    public float accuracy;
    public int physicalNegation;
    public int fireNegation;
    public int coldNegation;
    public int lightningNegation;
    public int earthNegation;
    public int divineNegation;
    public int voidNegation;

    public float vsBossDamage;
    public float directHitDamage;

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
        onHitEffectsFromAbility = new List<TriggeredEffect>();
    }

    public AbilityOnHitDataContainer DeepCopy()
    {
        return (AbilityOnHitDataContainer)MemberwiseClone();
    }

    public bool DidEffectProc(EffectType effectType, int avoidance)
    {
        float chance = GetEffectChance(effectType);
        return Helpers.RollChance((chance - avoidance) / 100f);
    }


    public class OnHitStatusEffectData
    {
        public float chance = 0;
        public float effectiveness = 0;
        public float duration = 0;
    }
}