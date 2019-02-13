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
    public readonly List<AffixBonus> affixBonuses;
    [JsonProperty]
    public readonly Dictionary<GroupType, int> spawnWeight;
    [JsonProperty]
    public readonly List<GroupType> groupTypes;
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