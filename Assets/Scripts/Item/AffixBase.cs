using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class AffixBase
{
    [JsonProperty]
    public readonly int id;
    [JsonProperty]
    public readonly string name;
    [JsonProperty]
    public readonly string description;
    [JsonConverter(typeof(StringEnumConverter))][JsonProperty]
    public readonly AffixType affixType;
    [JsonProperty]
    public readonly int tier;
    [JsonProperty]
    public readonly List<AffixBonus> affixBonuses;
    [JsonProperty]
    public readonly int weight;
}

public class AffixBonus
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

public enum AffixType
{
    PREFIX,
    SUFFIX,
    ENCHANTMENT
}

public enum ModifyType
{
    ADDITIVE,
    MULTIPLY,
    SET
}