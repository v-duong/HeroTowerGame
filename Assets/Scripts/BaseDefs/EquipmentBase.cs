using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class EquipmentBase
{
    [JsonProperty] 
    public readonly int id;
    [JsonProperty] 
    public readonly string name;
    [JsonProperty] 
    public readonly int dropLevel;
    [JsonProperty] 
    public readonly int armor;
    [JsonProperty] 
    public readonly int shield;
    [JsonProperty] 
    public readonly int dodge;
    [JsonProperty] 
    public readonly int magicDodge;
    [JsonProperty] 
    public readonly float regen;
    [JsonProperty]
    public readonly int minDamage;
    [JsonProperty]
    public readonly int maxDamage;
    [JsonProperty]
    public readonly float criticalChance;
    [JsonProperty]
    public readonly float weaponRange;
    [JsonProperty] 
    public readonly int strengthReq;
    [JsonProperty] 
    public readonly int intelligenceReq;
    [JsonProperty] 
    public readonly int agilityReq;
    [JsonProperty] 
    public readonly int willReq;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly EquipSlotType equipSlot;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly GroupType group;
    [JsonProperty]
    public readonly bool hasInnate;
    [JsonProperty]
    public readonly int innateAffixId;
}

public enum EquipSlotType
{
    WEAPON,
    OFF_HAND,
    BODY_ARMOR,
    HEADGEAR,
    GLOVES,
    BOOTS,
    BELT,
    RING,
    NECKLACE
}

