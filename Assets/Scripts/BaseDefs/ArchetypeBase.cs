using Newtonsoft.Json;
using System.Collections.Generic;

public class ArchetypeBase
{
    [JsonProperty]
    public readonly string idName;

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
    public readonly int spawnWeight;

    [JsonProperty]
    public readonly List<ArchetypeSkillNode> nodeList;

    public ArchetypeSkillNode GetNode(int nodeId)
    {
        return nodeList.Find(x => x.id == nodeId);
    }

    public List<AbilityBase> GetArchetypeAbilities()
    {
        List<AbilityBase> ret = new List<AbilityBase>();
        foreach (ArchetypeSkillNode node in nodeList)
        {
            if (node.type == NodeType.ABILITY)
                ret.Add(ResourceManager.Instance.GetAbilityBase( node.abilityId));
        }
        return ret;
    }
}