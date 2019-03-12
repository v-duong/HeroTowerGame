using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AbilityBase
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly string name;
    [JsonProperty]
    public readonly string description;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly AbilityType abilityType;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly AbilityShotType abilityShotType;
    [JsonProperty]
    public readonly float cooldown;
    [JsonProperty]
    public readonly float targetRange;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetType targetsAllies;
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
    public readonly List<AbilityDamageBase> damageLevels;
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
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly ElementType elementType;
    [JsonProperty]
    public readonly List<Vector2> damage;
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
