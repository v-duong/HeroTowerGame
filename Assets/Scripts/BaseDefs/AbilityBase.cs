using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

[Serializable]
public class AbilityBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityType abilityType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityShotType abilityShotType;

    [JsonProperty]
    public readonly float cooldown;

    [JsonProperty]
    public readonly float targetRange;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetType targetType;

    [JsonProperty]
    public readonly float projectileSpeed;

    [JsonProperty]
    public readonly float projectileSize;

    [JsonProperty]
    public readonly int projectileCount;

    [JsonProperty]
    public readonly float areaRadius;

    [JsonProperty]
    public readonly float areaLength;

    [JsonProperty]
    public readonly float hitscanDelay;

    [JsonProperty]
    public readonly float weaponMultiplier;

    [JsonProperty]
    public readonly float weaponMultiplierScaling;

    [JsonProperty]
    public readonly Dictionary<ElementType, AbilityDamageBase> damageLevels;

    [JsonProperty]
    public readonly float flatDamageMultiplier;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<GroupType> groupTypes;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<GroupType> weaponRestrictions;

    [JsonProperty]
    public readonly List<ScalingBonusProperty> bonusProperties;

    [JsonProperty]
    public readonly string effectSprite;

    [JsonProperty]
    public readonly LinkedAbilityData linkedAbility;
}

[Serializable]
public class AbilityDamageBase
{
    [JsonProperty]
    public readonly List<MinMaxRange> damage;
}

[Serializable]
public class MinMaxRange
{
    [JsonProperty]
    public int min;

    [JsonProperty]
    public int max;

    public void SetMinMax(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public void Clear()
    {
        min = 0;
        max = 0;
    }

    public bool IsZero()
    {
        if (min == 0 && max == 0)
            return true;
        else
            return false;
    }
}

[Serializable]
public class LinkedAbilityData
{
    [JsonProperty]
    public readonly int abilityId;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityLinkType type;

    [JsonProperty]
    public readonly float time;
}

[Serializable]
public class AbilityEffectData
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly EffectType effect;

    [JsonProperty]
    public readonly float chanceToApply;

    [JsonProperty]
    public readonly float effectPower;
}

public enum AbilityType
{
    ATTACK,
    SPELL,
    AURA,
    SELF_BUFF,
}

public enum DamageType
{
    DIRECT,
    DOT,
    PURE
}

public enum AbilityLinkType
{
    ON_HIT,
    TIME,
    ON_FADE,
}

public enum AbilityLinkInheritType
{
    NO_INHERITANCE,
    INHERIT_DAMAGE
}

public enum AbilityShotType
{
    PROJECTILE,
    HITSCAN,
    RADIAL_AOE,
    LINEAR_AOE
}

public enum AbilityTargetType
{
    ENEMY,
    ALLY,
    ALL,
    NONE
}