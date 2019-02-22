using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class ArchetypeBase
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string name;
    [JsonProperty]
    public readonly string text;
    [JsonProperty]
    public readonly int stars;
    [JsonProperty]
    public readonly int dropLevel;
    [JsonProperty]
    public readonly float healthGrowth;
    [JsonProperty]
    public readonly float soulPointGrowth;
    [JsonProperty]
    public readonly float strengthGrowth;
    [JsonProperty]
    public readonly float intelligenceGrowth;
    [JsonProperty]
    public readonly float agilityGrowth;
    [JsonProperty]
    public readonly float willGrowth;
    [JsonProperty]
    public readonly List<ArchetypeSkillNode> nodeList;
}


public class ArchetypeSkillNode
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string name;
    [JsonProperty]
    public readonly string desc;
    [JsonProperty]
    public readonly int initialLevel;
    [JsonProperty]
    public readonly int maxLevel;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly NodeType type;
    [JsonProperty]
    public readonly List<ArchetypeNodeBonus> bonuses;
    [JsonProperty]
    public Vector2 nodePosition;
    [JsonProperty]
    public List<ArchetypeNodeRequirements> requirements;
}

public class ArchetypeNodeBonus
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusType bonusType;
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;
    [JsonProperty]
    public readonly int intialValue;
    [JsonProperty]
    public readonly int growthValue;
}

public class ArchetypeNodeRequirements
{
    [JsonProperty]
    public readonly ArchetypeSkillNode node;
    [JsonProperty]
    public readonly int requiredLevel;
}

public enum NodeType
{
    LESSER,
    GREATER,
    MASTER
}