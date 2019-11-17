using System;
using System.Collections.Generic;

public class PlayerStats
{
    public const float EXP_STOCK_RATE = 0.5f;
    public const int HERO_TEAM_MAX_NUM = 5;
    public const int HERO_TEAM_MAX_HEROES = 5;
    public readonly static int maxEquipInventory = 250;
    public readonly static int maxArchetypeInventory = 100;
    public readonly static int maxAbilityInventory = 100;
    public readonly static int maxHeroes = 100;
    public readonly static int maxExpStock = 3000000;
    public readonly static int maxItemFragments = 5000000;
    public readonly static int maxArchetypeFragments = 1000;

    public int ItemFragments { get; private set; }
    public int ArchetypeFragments { get; private set; }
    public int ExpStock { get; private set; }

    public void SetExpStock(int value) => ExpStock = value;

    public Dictionary<ConsumableType, int> consumables;

    private List<Equipment> equipmentInventory;
    private List<ArchetypeItem> archetypeInventory;
    private List<AbilityCoreItem> abilityStorageInventory;
    private List<HeroData> heroList;
    public Dictionary<string, int> stageClearInfo;

    public IList<Equipment> EquipmentInventory
    {
        get
        {
            return equipmentInventory.AsReadOnly();
        }
    }

    public IList<ArchetypeItem> ArchetypeInventory
    {
        get
        {
            return archetypeInventory.AsReadOnly();
        }
    }

    public IList<AbilityCoreItem> AbilityInventory
    {
        get
        {
            return abilityStorageInventory.AsReadOnly();
        }
    }

    public IList<HeroData> HeroList
    {
        get
        {
            return heroList.AsReadOnly();
        }
    }

    public List<HeroData[]> heroTeams;

    public PlayerStats()
    {
        ItemFragments = 0;
        ArchetypeFragments = 0;
        ExpStock = 0;
        consumables = new Dictionary<ConsumableType, int>();
        foreach (ConsumableType c in Enum.GetValues(typeof(ConsumableType)))
        {
            consumables.Add(c, 0);
        }
        equipmentInventory = new List<Equipment>();
        archetypeInventory = new List<ArchetypeItem>();
        abilityStorageInventory = new List<AbilityCoreItem>();
        stageClearInfo = new Dictionary<string, int>();
        heroList = new List<HeroData>();
        heroTeams = new List<HeroData[]>();
        for (int i = 0; i < HERO_TEAM_MAX_NUM; i++)
        {
            heroTeams.Add(new HeroData[HERO_TEAM_MAX_HEROES]);
        }
    }

    public Equipment GetEquipmentByGuid(Guid id)
    {
        return equipmentInventory.Find(x => x.Id == id);
    }

    public void SetHeroToTeamSlot(HeroData hero, int selectedTeam, int selectedSlot)
    {
        if (hero.assignedTeam != -1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (heroTeams[hero.assignedTeam][i] == hero)
                    heroTeams[hero.assignedTeam][i] = null;
            }
        }
        heroTeams[selectedTeam][selectedSlot] = hero;
        hero.assignedTeam = selectedTeam;
    }

    public bool AddEquipmentToInventory(Equipment newEquipment)
    {
        if (equipmentInventory.Contains(newEquipment))
            return false;
        equipmentInventory.Add(newEquipment);
        return true;
    }

    public bool AddArchetypeToInventory(ArchetypeItem newArchetype)
    {
        if (archetypeInventory.Contains(newArchetype))
            return false;
        archetypeInventory.Add(newArchetype);
        return true;
    }

    public bool AddAbilityToInventory(AbilityCoreItem newAbility)
    {
        if (abilityStorageInventory.Contains(newAbility))
            return false;
        abilityStorageInventory.Add(newAbility);
        return true;
    }

    public bool AddHeroToList(HeroData hero)
    {
        if (heroList.Contains(hero))
            return false;
        heroList.Add(hero);
        return true;
    }

    public bool RemoveEquipmentFromInventory(Equipment equip)
    {
        equipmentInventory.Remove(equip);
        return true;
    }

    public bool RemoveArchetypeFromInventory(ArchetypeItem archetype)
    {
        archetypeInventory.Remove(archetype);
        return true;
    }

    public bool RemoveAbilityFromInventory(AbilityCoreItem newAbility)
    {
        abilityStorageInventory.Remove(newAbility);
        return true;
    }

    public bool RemoveHeroFromList(HeroData hero)
    {
        heroList.Remove(hero);
        return true;
    }

    public void ModifyExpStock(int value)
    {
        ExpStock += value;
        if (ExpStock < 0)
            ExpStock = 0;
        if (ExpStock > maxExpStock)
            ExpStock = maxExpStock;
    }

    public void ModifyItemFragments(int value)
    {
        ItemFragments += value;
        if (ItemFragments < 0)
            ItemFragments = 0;
        if (ItemFragments > maxItemFragments)
            ItemFragments = maxItemFragments;
    }

    public void SetItemFragments(int value)
    {
        ItemFragments = 0;
        ModifyItemFragments(value);
    }

    public void ModifyArchetypeFragments(int value)
    {
        ArchetypeFragments += value;
        if (ArchetypeFragments < 0)
            ArchetypeFragments = 0;
        if (ArchetypeFragments > maxArchetypeFragments)
            ArchetypeFragments = maxArchetypeFragments;
    }

    public void SetArchetypeFragments(int value)
    {
        ArchetypeFragments = 0;
        ModifyArchetypeFragments(value);
    }

    public void ClearEquipmentInventory()
    {
        equipmentInventory.Clear();
    }

    public void ClearAbilityInventory()
    {
        abilityStorageInventory.Clear();
    }

    public void ClearHeroList()
    {
        heroList.Clear();
    }

    public void ClearArchetypeItemInventory()
    {
        archetypeInventory.Clear();
    }

    public void AddToStageClearCount(string stageId)
    {
        if (!stageClearInfo.ContainsKey(stageId))
            stageClearInfo.Add(stageId, 0);
        stageClearInfo[stageId]++;
    }

    public bool IsStageUnlocked(string stageId)
    {
        if (!stageClearInfo.ContainsKey(stageId) || stageClearInfo[stageId] == 0)
            return false;
        else
            return true;
    }

    public int GetStageClearCount(string stageId)
    {
        if (!stageClearInfo.ContainsKey(stageId))
            return 0;
        else
            return stageClearInfo[stageId];
    }
}

public enum ConsumableType
{
    LOW_TIER_UPGRADER,
    RARE_TO_EPIC,
    AFFIX_REROLLER,
    AFFIX_CRAFTER
}