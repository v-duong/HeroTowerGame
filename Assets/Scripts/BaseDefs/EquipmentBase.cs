﻿using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class EquipmentBase
{

    [JsonProperty]
    public readonly string idName;
    [JsonProperty] 
    public readonly int dropLevel;
    [JsonProperty] 
    public readonly int armor;
    [JsonProperty] 
    public readonly int shield;
    [JsonProperty] 
    public readonly int dodgeRating;
    [JsonProperty] 
    public readonly int resolveRating;
    [JsonProperty] 
    public readonly int sellValue;
    [JsonProperty]
    public readonly int minDamage;
    [JsonProperty]
    public readonly int maxDamage;
    [JsonProperty]
    public readonly float criticalChance;
    [JsonProperty]
    public readonly float attackSpeed;
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
    public readonly string innateAffixId;
    [JsonProperty]
    public readonly int spawnWeight;

    public string LocalizedName => LocalizationManager.Instance.GetLocalizationText_Equipment(idName);
}

public class UniqueBase : EquipmentBase
{
    [JsonProperty]
    public readonly List<AffixBase> fixedUniqueAffixes;
    [JsonProperty]
    public readonly List<AffixBase> randomUniqueAffixes;
    [JsonProperty]
    public readonly int randomAffixesToSpawn;
    [JsonProperty]
    public readonly int uniqueVersion;
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
    NECKLACE,
    RING_SLOT_1,
    RING_SLOT_2,
    RING,
}

