using System.Collections.Generic;
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

    public string GetBonusInfoString(int currentLevel)
    {
        string s = "";
        foreach(NodeScalingBonusProperty bonus in bonuses)
        {
            float value = bonus.GetBonusValueAtLevel(currentLevel, maxLevel);
            if (value == 0 && (bonus.modifyType != ModifyType.MULTIPLY || bonus.modifyType != ModifyType.FIXED_TO))
                continue;

            s += LocalizationManager.Instance.GetLocalizationText_BonusType(bonus.bonusType, bonus.modifyType, value);
        }
        return s;
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

    public float GetBonusValueAtLevel(int level, int maxLevel)
    {
        if (maxLevel == 1)
            return growthValue;
        else if (level != maxLevel)
            return growthValue * level;
        else if (level == maxLevel)
            return growthValue * (maxLevel - 1) + finalLevelValue;
        else
            return 0;
    }
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