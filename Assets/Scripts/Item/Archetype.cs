﻿using System.Collections.Generic;

public class Archetype : Item
{
    public ArchetypeBase Base { get { return ResourceManager.Instance.GetArchetypeBase(BaseId); } }
    private int BaseId { get; set; }

    public int level;
    public int experience;
    public int skillPoints;
    public HeroActor equippedToHero;
    public List<Affix> innate;
    public List<Affix> enchantments;

    private Dictionary<ArchetypeSkillNode, NodeLevel> nodeLevels;

    public Archetype(ArchetypeBase b)
    {
        BaseId = b.id;
        itemType = GroupType.ARCHETYPE;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
        enchantments = new List<Affix>();
        innate = new List<Affix>();
        nodeLevels = new Dictionary<ArchetypeSkillNode, NodeLevel>();
        foreach (var node in b.nodeList)
        {
            NodeLevel n = new NodeLevel
            {
                level = node.initialLevel,
                bonusLevels = 0
            };
            nodeLevels.Add(node, n);
        }
    }

    public int GetMaxLevel()
    {
        switch (Rarity)
        {
            case RarityType.EPIC:
                return 50;

            case RarityType.RARE:
                return 40;

            case RarityType.UNCOMMON:
                return 30;

            case RarityType.NORMAL:
            default:
                return 20;
        }
    }

    public override bool UpgradeRarity()
    {
        if (Rarity == RarityType.EPIC)
            return false;
        Rarity++;
        return true;
    }

    public bool LevelUpNode(ArchetypeSkillNode node)
    {
        NodeLevel nodeLevel = nodeLevels[node];
        if (nodeLevel.level == node.maxLevel)
            return false;
        nodeLevel.level++;

        foreach (var bonus in node.bonuses)
        {
            if (nodeLevel.level == 1)
            {
                equippedToHero.AddStatBonus(bonus.intialValue, bonus.bonusType, bonus.modifyType);
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
        NodeLevel nodeLevel = nodeLevels[node];
        if (nodeLevel.level == node.initialLevel)
            return false;
        nodeLevel.level--;

        foreach (var bonus in node.bonuses)
        {
            if (nodeLevel.level == 0)
            {
                equippedToHero.RemoveStatBonus(bonus.intialValue, bonus.bonusType, bonus.modifyType);
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
        NodeLevel nodeLevel = nodeLevels[node];
        nodeLevel.bonusLevels += value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                equippedToHero.AddStatBonus(bonus.growthValue * value, bonus.bonusType, bonus.modifyType);
    }

    public void AddRemoveLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node];
        nodeLevel.bonusLevels -= value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                equippedToHero.RemoveStatBonus(bonus.growthValue * value, bonus.bonusType, bonus.modifyType);
    }
}