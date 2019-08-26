using System.Collections.Generic;
using UnityEngine;

public class HeroArchetypeData : IAbilitySource
{
    public const int LocalBonusStart = 0x900;
    public ArchetypeBase Base { get { return ResourceManager.Instance.GetArchetypeBase(BaseId); } }
    private string BaseId { get; set; }
    public int allocatedSkillPoints;
    public HeroData hero;

    public float HealthGrowth { get; private set; }
    public float SoulPointGrowth { get; private set; }
    public float StrengthGrowth { get; private set; }
    public float IntelligenceGrowth { get; private set; }
    public float AgilityGrowth { get; private set; }
    public float WillGrowth { get; private set; }
    public List<AbilityBase> AvailableAbilityList { get; }

    private Dictionary<int, NodeLevel> nodeLevels;

    public HeroArchetypeData(ArchetypeItem archetypeItem, HeroData hero)
    {
        BaseId = archetypeItem.Base.idName;

        HealthGrowth = Base.healthGrowth;
        SoulPointGrowth = Base.soulPointGrowth;
        StrengthGrowth = Base.strengthGrowth;
        IntelligenceGrowth = Base.intelligenceGrowth;
        AgilityGrowth = Base.agilityGrowth;
        WillGrowth = Base.willGrowth;
        this.hero = hero;
        AvailableAbilityList = new List<AbilityBase>();
        nodeLevels = new Dictionary<int, NodeLevel>();
        InitializeNodeLevels();
    }

    public void InitializeNodeLevels()
    {
        foreach (var node in Base.nodeList)
        {
            NodeLevel n = new NodeLevel
            {
                level = node.initialLevel,
                bonusLevels = 0
            };
            if (node.type == NodeType.ABILITY && n.level >= 1)
                AvailableAbilityList.Add(node.GetAbility());
            nodeLevels.Add(node.id, n);
        }
    }

    public int GetAbilityLevel()
    {
        if (hero.Level != 100)
            return (int)((hero.Level - 1) / 2d);
        else
            return 50;
    }

    public int GetNodeLevel(int id)
    {
        return nodeLevels[id].level;
    }

    public int GetNodeLevel(ArchetypeSkillNode node)
    {
        return nodeLevels[node.id].level;
    }

    public bool IsNodeMaxLevel(ArchetypeSkillNode node)
    {
        if (nodeLevels[node.id].level == node.maxLevel)
            return true;
        else
            return false;
    }

    public bool LevelUpNode(ArchetypeSkillNode node)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        if (nodeLevel.level == node.maxLevel)
            return false;
        nodeLevel.level++;
        if (node.type == NodeType.ABILITY)
            AvailableAbilityList.Add(node.GetAbility());
        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (nodeLevel.level == 1)
            {
                bonusValue = bonus.growthValue;
                if (nodeLevel.bonusLevels > 0 && node.maxLevel > 1)
                    bonusValue += bonus.growthValue * nodeLevel.bonusLevels;
            }
            else if (nodeLevel.level == node.maxLevel)
                bonusValue = bonus.finalLevelValue;
            else
                bonusValue = bonus.growthValue;

            hero.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
        }
        hero.UpdateActorData();
        return true;
    }

    public bool DelevelNode(ArchetypeSkillNode node)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        if (nodeLevel.level == node.initialLevel)
            return false;
        
        if (node.type == NodeType.ABILITY)
            AvailableAbilityList.Remove(node.GetAbility());

        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (nodeLevel.level == 1)
            {
                bonusValue = bonus.growthValue;
                if (nodeLevel.bonusLevels > 0 && node.maxLevel > 1)
                    bonusValue += bonus.growthValue * nodeLevel.bonusLevels;
            }
            else if (nodeLevel.level == node.maxLevel)
                bonusValue = bonus.finalLevelValue;
            else
                bonusValue = bonus.growthValue;

            hero.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
        }

        nodeLevel.level--;

        hero.UpdateActorData();
        return true;
    }

    public void AddBonusLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        nodeLevel.bonusLevels += value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                hero.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonus.growthValue * value);
    }

    public void RemoveBonusLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        nodeLevel.bonusLevels -= value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                hero.RemoveArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonus.growthValue * value);
    }

    public bool ContainsAbility(string id)
    {
        return true;
    }
}