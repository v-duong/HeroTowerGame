using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroArchetypeData
{
    public ArchetypeBase Base { get { return ResourceManager.Instance.GetArchetypeBase(BaseId); } }
    private string BaseId { get; set; }
    public int allocatedSkillPoints;
    public HeroData equippedToHero;

    public float healthGrowth;
    public float soulPointGrowth;
    public float strengthGrowth;
    public float intelligenceGrowth;
    public float agilityGrowth;
    public float willGrowth;

    private Dictionary<int, NodeLevel> nodeLevels;

    public HeroArchetypeData(ArchetypeItem archetypeItem)
    {
        BaseId = archetypeItem.Base.idName;

        healthGrowth = Base.healthGrowth;
        soulPointGrowth = Base.soulPointGrowth;
        strengthGrowth = Base.strengthGrowth;
        intelligenceGrowth = Base.intelligenceGrowth;
        agilityGrowth = Base.agilityGrowth;
        willGrowth = Base.willGrowth;

        nodeLevels = new Dictionary<int, NodeLevel>();
        foreach (var node in Base.nodeList)
        {
            NodeLevel n = new NodeLevel
            {
                level = node.initialLevel,
                bonusLevels = 0
            };
            nodeLevels.Add(node.id, n);
        }
    }

    public bool LevelUpNode(ArchetypeSkillNode node)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        if (nodeLevel.level == node.maxLevel)
            return false;
        nodeLevel.level++;

        foreach (var bonus in node.bonuses)
        {
            if (nodeLevel.level == 1)
            {
                equippedToHero.AddStatBonus(bonus.initialValue, bonus.bonusType, bonus.modifyType);
                if (nodeLevel.bonusLevels > 0)
                {
                    equippedToHero.AddStatBonus(bonus.growthValue * nodeLevel.bonusLevels, bonus.bonusType, bonus.modifyType);
                }
            }
            else
                equippedToHero.AddStatBonus(bonus.growthValue, bonus.bonusType, bonus.modifyType);
        }

        return true;
    }

    public bool DelevelNode(ArchetypeSkillNode node)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        if (nodeLevel.level == node.initialLevel)
            return false;
        nodeLevel.level--;

        foreach (var bonus in node.bonuses)
        {
            if (nodeLevel.level == 0)
            {
                equippedToHero.RemoveStatBonus(bonus.initialValue, bonus.bonusType, bonus.modifyType);
                if (nodeLevel.bonusLevels > 0)
                {
                    equippedToHero.RemoveStatBonus(bonus.growthValue * nodeLevel.bonusLevels, bonus.bonusType, bonus.modifyType);
                }
            }
            else
                equippedToHero.RemoveStatBonus(bonus.growthValue, bonus.bonusType, bonus.modifyType);
        }

        return true;
    }

    public void AddBonusLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        nodeLevel.bonusLevels += value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                equippedToHero.AddStatBonus(bonus.growthValue * value, bonus.bonusType, bonus.modifyType);
    }

    public void AddRemoveLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        nodeLevel.bonusLevels -= value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                equippedToHero.RemoveStatBonus(bonus.growthValue * value, bonus.bonusType, bonus.modifyType);
    }

    public bool ContainsAbility(string id)
    {
        return true;
    }
}
