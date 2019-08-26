using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public class AffixBase
{
    [JsonProperty]
    public readonly string idName;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly AffixType affixType;

    [JsonProperty]
    public readonly int tier;

    [JsonProperty]
    public readonly int spawnLevel;

    [JsonProperty]
    public readonly List<AffixBonusProperty> affixBonuses;

    [JsonProperty]
    public readonly List<AffixWeight> spawnWeight;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<GroupType> groupTypes;

    public string AffixBonusTypeString { get; private set; }

    public void SetAffixBonusTypeString()
    {
        int i = 0;
        string temp = "";
        foreach (AffixBonusProperty x in affixBonuses)
        {
            temp += x.bonusType.ToString();
            temp += "_";
            temp += x.modifyType.ToString();
            if (i + 1 != affixBonuses.Count)
            {
                temp += "_";
                i++;
            }
        }
        AffixBonusTypeString = temp;
    }
}

public struct AffixBonusProperty
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly BonusType bonusType;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly ModifyType modifyType;

    [JsonProperty]
    public readonly float minValue;

    [JsonProperty]
    public readonly float maxValue;

    [JsonProperty]
    public readonly bool readAsFloat;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly GroupType restriction;
}

public struct AffixWeight
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly GroupType type;

    [JsonProperty]
    public readonly int weight;
}

public enum AffixType
{
    PREFIX,
    SUFFIX,
    ENCHANTMENT,
    INNATE,
    MONSTERMOD
}

public enum ModifyType
{
    ADDITIVE,       //all sources add together before modifying
    MULTIPLY,       //all sources multiply together before modifying
    FIXED_TO,            //sets value to modifier value, ignores all other increases
    FLAT_ADDITION   //adds to base before any other calculation
}