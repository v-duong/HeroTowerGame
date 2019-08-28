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
    public readonly float attacksPerSec;

    [JsonProperty]
    public readonly float targetRange;

    [JsonProperty]
    public readonly float baseCritical;

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
    public readonly int projectileSpread;

    [JsonProperty]
    public readonly bool doesProjectileSpread;

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
    private readonly List<GroupType> groupTypes;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<GroupType> weaponRestrictions;

    [JsonProperty]
    public readonly List<AbilityScalingBonusProperty> bonusProperties;

    [JsonProperty]
    public readonly string effectSprite;

    [JsonProperty]
    public readonly LinkedAbilityData linkedAbility;

    [JsonProperty]
    public readonly bool hasLinkedAbility;
    [JsonProperty]
    public readonly bool useWeaponRangeForTargeting;
    [JsonProperty]
    public readonly bool useWeaponRangeForAOE;
    [JsonProperty]
    public readonly bool useBothWeaponsForDual;

    public MinMaxRange GetDamageAtLevel(ElementType e, int level)
    {
        if (damageLevels.TryGetValue(e, out AbilityDamageBase damageBase))
        {
            return damageBase.damage[level];
        }
        else
        {
            return null;
        }
    }

    public IList<GroupType> GetGroupTypes()
    {
        return groupTypes.AsReadOnly();
    }
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

public class LinkedAbilityData
{
    [JsonProperty]
    public readonly string abilityId;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityLinkType type;

    [JsonProperty]
    public readonly float time;

    [JsonProperty]
    public readonly bool inheritsDamage;

    [JsonProperty]
    public readonly float inheritDamagePercent;
    [JsonProperty]
    public readonly float inheritDamagePercentScaling;
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
    NONE,
    ON_EVERY_HIT,
    ON_FINAL_HIT,
    ON_FIRST_HIT,
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
    HITSCAN_SINGLE,
    ARC_AOE,
    RADIAL_AOE,
    NOVA_AOE,
    NOVA_ARC_AOE,
    LINEAR_AOE
}

public enum AbilityTargetType
{
    ENEMY,
    ALLY,
    ALL,
    NONE
}