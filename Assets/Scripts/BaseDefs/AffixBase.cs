using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class AffixBase
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string name;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly AffixType affixType;
    [JsonProperty]
    public readonly int tier;
    [JsonProperty]
    public readonly int spawnLevel;
    [JsonProperty]
    public readonly List<AffixBonusBase> affixBonuses;
    [JsonProperty]
    public readonly List<AffixWeight> spawnWeight;
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public readonly List<GroupType> groupTypes;
    public string BonusTagType { get; private set; }

    public void SetBonusTagType()
    {
        int i = 0;
        string temp = "";
        foreach (AffixBonusBase x in affixBonuses)
        {
            temp += x.bonusType.ToString();
            temp += "_";
            temp += x.modifyType.ToString();
            if (i+1 != affixBonuses.Count)
            {
                temp += "_";
                i++;
            }
        }
        BonusTagType = temp;
        Debug.Log(temp);
    }
}

public class AffixBonusBase
{
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly BonusType bonusType;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly ModifyType modifyType;
    [JsonProperty]
    public readonly int minValue;
    [JsonProperty]
    public readonly int maxValue;
}

public class AffixWeight
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    public readonly GroupType groupType;
    [JsonProperty]
    public readonly int weight;
}

public enum AffixType
{
    PREFIX,
    SUFFIX,
    ENCHANTMENT,
    INNATE
}

public enum ModifyType
{
    ADDITIVE,       //all sources add together before modifying
    MULTIPLY,       //all sources multiply together before modifying
    SET,            //sets value to modifier value, ignores all other increases
    FLAT_ADDITION   //adds to base before any other calculation
}