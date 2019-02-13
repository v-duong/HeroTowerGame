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
    public int dropLevel;
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
    public readonly int strengthReq;
    [JsonProperty] 
    public readonly int intelligenceReq;
    [JsonProperty] 
    public readonly int agilityReq;
    [JsonProperty] 
    public readonly int willReq;
    [JsonProperty]
    public readonly GroupType equipSlot;
    [JsonProperty]
    public readonly GroupType group;
}
