using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class TriggeredEffectBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly TriggerType triggerType;

    [JsonProperty]
    public readonly float triggerValue;

    [JsonProperty]
    public readonly GroupType restriction;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetType effectTargetType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly EffectType effectType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusType statBonusType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType statModifyType;

    [JsonProperty]
    public readonly float triggerChance;

    [JsonProperty]
    public readonly float effectMinValue;

    [JsonProperty]
    public readonly float effectMaxValue;

    [JsonProperty]
    public readonly float effectDuration;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ElementType effectElement;

    [JsonProperty]
    public readonly bool readAsFloat;
}


public enum TriggerType
{
    ON_HIT,
    WHEN_HIT_BY,
    WHEN_HITTING,
    ON_KILL,
    ON_HIT_KILL,
    HEALTH_THRESHOLD,
    SHIELD_THRESHOLD,
    SOULPOINT_THRESHOLD,
    ON_BLOCK,
    ON_DODGE,
    ON_PARRY,
    ON_PHASING,
    ON_DEATH,
}
