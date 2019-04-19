using System.Collections.Generic;

public class HeroArchetypeData
{
    public ArchetypeBase Base { get { return ResourceManager.Instance.GetArchetypeBase(BaseId); } }
    private string BaseId { get; set; }
    public int allocatedSkillPoints;
    public HeroData hero;

    public float healthGrowth;
    public float soulPointGrowth;
    public float strengthGrowth;
    public float intelligenceGrowth;
    public float agilityGrowth;
    public float willGrowth;

    private Dictionary<int, NodeLevel> nodeLevels;

    public HeroArchetypeData(ArchetypeItem archetypeItem, HeroData hero)
    {
        BaseId = archetypeItem.Base.idName;

        healthGrowth = Base.healthGrowth;
        soulPointGrowth = Base.soulPointGrowth;
        strengthGrowth = Base.strengthGrowth;
        intelligenceGrowth = Base.intelligenceGrowth;
        agilityGrowth = Base.agilityGrowth;
        willGrowth = Base.willGrowth;
        this.hero = hero;

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
            nodeLevels.Add(node.id, n);
        }
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

        foreach (var bonus in node.bonuses)
        {
            if (nodeLevel.level == 1)
            {
                hero.AddArchetypeStatBonus(bonus.initialValue, bonus.bonusType, bonus.modifyType);
                if (nodeLevel.bonusLevels > 0)
                {
                    hero.AddArchetypeStatBonus(bonus.growthValue * nodeLevel.bonusLevels, bonus.bonusType, bonus.modifyType);
                }
            }
            else
                hero.AddArchetypeStatBonus(bonus.growthValue, bonus.bonusType, bonus.modifyType);
        }
        hero.UpdateHeroAllStats();
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
                hero.RemoveArchetypeStatBonus(bonus.initialValue, bonus.bonusType, bonus.modifyType);
                if (nodeLevel.bonusLevels > 0)
                {
                    hero.RemoveArchetypeStatBonus(bonus.growthValue * nodeLevel.bonusLevels, bonus.bonusType, bonus.modifyType);
                }
            }
            else
                hero.RemoveArchetypeStatBonus(bonus.growthValue, bonus.bonusType, bonus.modifyType);
        }
        hero.UpdateHeroAllStats();
        return true;
    }

    public void AddBonusLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        nodeLevel.bonusLevels += value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                hero.AddArchetypeStatBonus(bonus.growthValue * value, bonus.bonusType, bonus.modifyType);
    }

    public void RemoveBonusLevels(ArchetypeSkillNode node, int value)
    {
        NodeLevel nodeLevel = nodeLevels[node.id];
        nodeLevel.bonusLevels -= value;

        foreach (var bonus in node.bonuses)
            if (nodeLevel.level >= 1)
                hero.RemoveArchetypeStatBonus(bonus.growthValue * value, bonus.bonusType, bonus.modifyType);
    }

    public bool ContainsAbility(string id)
    {
        return true;
    }
}