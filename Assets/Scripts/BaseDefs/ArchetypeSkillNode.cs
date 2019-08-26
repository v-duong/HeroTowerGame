﻿using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class ArchetypeSkillNode
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly int initialLevel;
    [JsonProperty]
    public readonly int maxLevel;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly NodeType type;
    [JsonProperty]
    public readonly List<NodeScalingBonusProperty> bonuses;
    [JsonProperty]
    public readonly string abilityId;
    [JsonProperty]
    public readonly Vector2 nodePosition;
    [JsonProperty]
    public readonly string iconPath;
    [JsonProperty]
    public readonly List<int> children;

    private AbilityBase abilityBase;
    public AbilityBase GetAbility()
    {
        if (string.IsNullOrEmpty(abilityId))
        {
            return null;
        }
        else if (abilityBase == null)
        {
            abilityBase = ResourceManager.Instance.GetAbilityBase(abilityId);
        }
        return abilityBase;
    }
}


public struct NodeScalingBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusType bonusType;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;
    [JsonProperty]
    public readonly float growthValue;
    [JsonProperty]
    public readonly float finalLevelValue;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly GroupType restriction;
}

public struct AbilityScalingBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusType bonusType;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;
    [JsonProperty]
    public readonly float initialValue;
    [JsonProperty]
    public readonly float growthValue;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly GroupType restriction;
}

public enum NodeType
{
    LESSER,
    GREATER,
    MASTER,
    ABILITY
}


public class NodeLevel
{
    public int level;
    public int bonusLevels;
}