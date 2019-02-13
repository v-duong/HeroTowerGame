using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class AbilityBase
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string name;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly AbilityType abilityType;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly AbilityShotType abilityShotType;
    [JsonProperty]
    public readonly float baseCooldown;
    [JsonProperty]
    public readonly float baseTargetRange;
    [JsonProperty]
    public readonly float baseProjectileSpeed;
    [JsonProperty]
    public readonly float baseProjectileSize;
    [JsonProperty]
    public readonly float baseWeaponMultiplier;
    [JsonProperty]
    public readonly float weaponMultiplierScaling;
    [JsonProperty]
    public readonly List<AbilityBaseDamageEntry> baseDamage;
    [JsonProperty]
    public readonly List<GroupType> groupTypes;
}

[Serializable]
public class AbilityBaseDamageEntry
{
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly ElementType elementType;
    [JsonProperty]
    public readonly int baseMin;
    [JsonProperty]
    public readonly int baseMax;
}


public enum AbilityType
{
    ATTACK,
    SPELL,
    AURA,
    BUFF
}

public enum AbilityShotType
{
    PROJECTILE,
    PROJECTILE_NOVA,
    HITSCAN,
    RADIAL_AOE,
    LINEAR_AOE
}
