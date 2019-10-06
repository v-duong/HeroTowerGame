using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

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
    public readonly float projectileLifespanMulti;

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
    public readonly List<AbilityScalingAddedEffect> appliedEffects;
    [JsonProperty]
    public readonly List<TriggeredEffectBonusProperty> triggeredEffects;

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
    [JsonProperty]
    public int hitCount;
    [JsonProperty]
    public float hitDamageModifier;
    [JsonProperty]
    public float delayBetweenHits;

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


public class AbilityDamageBase
{
    [JsonProperty]
    public readonly List<MinMaxRange> damage;
}

public struct LinkedAbilityData
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
    NON_ABILITY
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
    TIME_REPEAT,
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
    HITSCAN_MULTI,
    ARC_AOE,
    FORWARD_MOVING_ARC,
    RADIAL_AOE,
    FORWARD_MOVING_RADIAL,
    NOVA_AOE,
    NOVA_ARC_AOE,
    LINEAR_AOE,
    FORWARD_MOVING_LINEAR,
    PROJECTILE_NOVA
}

public enum AbilityTargetType
{
    ENEMY,
    ALLY,
    ALL,
    SELF,
    NONE
}

public class AbilityScalingAddedEffect
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly EffectType type;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusType bonusType;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AbilityTargetType targetType;
    [JsonProperty]
    public readonly float chance;
    [JsonProperty]
    public readonly float initialValue;
    [JsonProperty]
    public readonly float growthValue;
    [JsonProperty]
    public readonly float duration;
    [JsonProperty]
    public readonly int stacks;
    [JsonProperty]
    public readonly bool useLastRoll;
}