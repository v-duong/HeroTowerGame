using System.Collections;
using System.Collections.Generic;
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

