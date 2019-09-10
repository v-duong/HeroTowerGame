using System;
using System.Collections.Generic;

public class HeroArchetypeData : IAbilitySource
{
    public const int LocalBonusStart = 0x900;

    public Guid Id { get; private set; }
    public ArchetypeBase Base { get; private set; }
    public HeroData hero;

    public float HealthGrowth { get; private set; }
    public float SoulPointGrowth { get; private set; }
    public float StrengthGrowth { get; private set; }
    public float IntelligenceGrowth { get; private set; }
    public float AgilityGrowth { get; private set; }
    public float WillGrowth { get; private set; }
    public ArchetypeLeveledAbilityList AvailableAbilityList { get; }

    public AbilitySourceType AbilitySourceType => AbilitySourceType.ARCHETYPE;
    public string SourceName => Base.idName;

    public Guid SourceId => Id;

    private Dictionary<int, int> nodeLevels;

    public Dictionary<int, int> NodeLevels => new Dictionary<int, int>(nodeLevels);

    public HeroArchetypeData(ArchetypeItem archetypeItem, HeroData hero)
    {
        Id = Guid.NewGuid();
        Base = archetypeItem.Base;
        this.hero = hero;
        AvailableAbilityList = new ArchetypeLeveledAbilityList();
        InitializeArchetypeData();
    }

    public HeroArchetypeData(SaveData.HeroArchetypeSaveData archetypeSaveData, HeroData hero)
    {
        Id = archetypeSaveData.id;
        Base = ResourceManager.Instance.GetArchetypeBase(archetypeSaveData.archetypeId);
        this.hero = hero;
        AvailableAbilityList = new ArchetypeLeveledAbilityList();
        InitializeArchetypeData();

        foreach (var nodeSaveData in archetypeSaveData.nodeLevelData)
        {
            LoadNodeLevelsFromSave(Base.GetNode(nodeSaveData.nodeId), nodeLevels[nodeSaveData.nodeId], nodeSaveData.level);
        }
    }

    public void InitializeArchetypeData()
    {
        HealthGrowth = Base.healthGrowth;
        SoulPointGrowth = Base.soulPointGrowth;
        StrengthGrowth = Base.strengthGrowth;
        IntelligenceGrowth = Base.intelligenceGrowth;
        AgilityGrowth = Base.agilityGrowth;
        WillGrowth = Base.willGrowth;
        nodeLevels = new Dictionary<int, int>();
        InitializeNodeLevels();
    }

    public void InitializeNodeLevels()
    {
        foreach (var node in Base.nodeList)
        {
            int level = node.initialLevel;
            if (node.type == NodeType.ABILITY && level >= 1)
                AvailableAbilityList.Add(node.GetAbility());
            else if (level == 1)
                foreach (var bonus in node.bonuses)
                {
                    hero.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonus.growthValue);
                }
            nodeLevels.Add(node.id, level);
        }
    }

    public int GetNodeLevel(int id)
    {
        return nodeLevels[id];
    }

    public int GetNodeLevel(ArchetypeSkillNode node)
    {
        return nodeLevels[node.id];
    }

    public bool IsNodeMaxLevel(ArchetypeSkillNode node)
    {
        if (nodeLevels[node.id] == node.maxLevel)
            return true;
        else
            return false;
    }

    private void LoadNodeLevelsFromSave(ArchetypeSkillNode node, int initalLevel, int setLevel)
    {
        if (node == null)
            return;
        if (initalLevel == setLevel)
            return;

        hero.ModifyArchetypePoints(-(setLevel - initalLevel));

        if (node.type == NodeType.ABILITY && setLevel >= 1)
            AvailableAbilityList.Add(node.GetAbility());

        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (setLevel > initalLevel)
            {
                if (setLevel == 1 && node.maxLevel == 1)
                {
                    bonusValue = bonus.growthValue;
                }
                else if (setLevel == node.maxLevel)
                {
                    int difference = setLevel - initalLevel - 1;
                    bonusValue = bonus.growthValue * difference + bonus.finalLevelValue;
                }
                else
                {
                    int difference = setLevel - initalLevel;
                    bonusValue = bonus.growthValue * difference;
                }
                hero.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
            }
        }
        nodeLevels[node.id] = setLevel;

    }

    public bool LevelUpNode(ArchetypeSkillNode node)
    {
        if (nodeLevels[node.id] == node.maxLevel)
            return false;
        nodeLevels[node.id]++;
        if (node.type == NodeType.ABILITY)
            AvailableAbilityList.Add(node.GetAbility());
        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (nodeLevels[node.id] == 1)
            {
                bonusValue = bonus.growthValue;
            }
            else if (nodeLevels[node.id] == node.maxLevel)
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
        if (nodeLevels[node.id] == node.initialLevel)
            return false;

        if (node.type == NodeType.ABILITY)
            AvailableAbilityList.Remove(node.GetAbility());

        foreach (var bonus in node.bonuses)
        {
            float bonusValue = 0f;
            if (nodeLevels[node.id] == 1)
            {
                bonusValue = bonus.growthValue;
            }
            else if (nodeLevels[node.id] == node.maxLevel)
                bonusValue = bonus.finalLevelValue;
            else
                bonusValue = bonus.growthValue;

            hero.AddArchetypeStatBonus(bonus.bonusType, bonus.restriction, bonus.modifyType, bonusValue);
        }

        nodeLevels[node.id]--;

        hero.UpdateActorData();
        return true;
    }

    public bool ContainsAbility(string id)
    {
        return true;
    }

    public void OnEquip(AbilityBase ability, HeroData hero, int slot)
    {
        foreach (var archetypeAbility in AvailableAbilityList)
        {
            if (archetypeAbility.abilityBase == ability)
            {
                archetypeAbility.equippedHero = hero;
                archetypeAbility.equippedSlot = slot;
                return;
            }
        }
    }

    public void OnUnequip(AbilityBase ability, HeroData hero, int slot)
    {
        foreach (var archetypeAbility in AvailableAbilityList)
        {
            if (archetypeAbility.abilityBase == ability)
            {
                archetypeAbility.equippedHero = null;
                return;
            }
        }
    }

    public Tuple<HeroData, int> GetEquippedHeroAndSlot(AbilityBase ability)
    {
        int slot = -1;
        foreach (var archetypeAbility in AvailableAbilityList)
        {
            if (archetypeAbility.abilityBase == ability)
            {
                if (archetypeAbility.equippedHero == null)
                    return null;
                slot = archetypeAbility.equippedSlot;
            }
        }
        return new Tuple<HeroData, int>(hero, slot);
    }

    public class ArchetypeLeveledAbilityList 
    {
        public class ArchetypeLeveledAbility
        {
            public AbilityBase abilityBase;
            public HeroData equippedHero;
            public int equippedSlot;

            public ArchetypeLeveledAbility(AbilityBase abilityBase)
            {
                this.abilityBase = abilityBase;
                equippedHero = null;
            }
        }

        private List<ArchetypeLeveledAbility> leveledAbilities;

        public ArchetypeLeveledAbilityList()
        {
            leveledAbilities = new List<ArchetypeLeveledAbility>();
        }

        public void Add(AbilityBase a)
        {
            leveledAbilities.Add(new ArchetypeLeveledAbility(a));
        }

        public void Remove(AbilityBase a)
        {
            ArchetypeLeveledAbility target = leveledAbilities.Find(x => x.abilityBase == a);
            leveledAbilities.Remove(target);
        }

        public IEnumerator<ArchetypeLeveledAbility> GetEnumerator()
        {
            return leveledAbilities.GetEnumerator();
        }
    }
}