using System;
using System.Collections.Generic;

public class PlayerStats
{
    public const float EXP_STOCK_RATE = 0.75f;
    public const int HERO_TEAM_MAX_NUM = 5;
    public const int HERO_TEAM_MAX_HEROES = 5;
    public readonly static int maxEquipInventory = 350;
    public readonly static int maxArchetypeInventory = 100;
    public readonly static int maxAbilityInventory = 100;
    public readonly static int maxHeroes = 100;
    public readonly static int maxExpStock = 3500000;
    public readonly static int maxItemFragments = 20000000;
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
    public Dictionary<int, bool> worldUnlockInfo;
    public int lastPlayedWorld;
    public bool hasSeenStartingMessage;

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
        lastPlayedWorld = 1;
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

        heroList = new List<HeroData>();
        heroTeams = new List<HeroData[]>();
        for (int i = 0; i < HERO_TEAM_MAX_NUM; i++)
        {
            heroTeams.Add(new HeroData[HERO_TEAM_MAX_HEROES]);
        }

        stageClearInfo = new Dictionary<string, int>() { { "stage1-1", 0 } };

        worldUnlockInfo = new Dictionary<int, bool>()
        {
            {1, true }
        };
    }

    public Equipment GetEquipmentByGuid(Guid id)
    {
        return equipmentInventory.Find(x => x.Id == id);
    }

    public void SetHeroToTeamSlot(HeroData hero, int selectedTeam, int selectedSlot)
    {
        if (hero == null)
        {
            if (heroTeams[selectedTeam][selectedSlot] != null)
            {
                heroTeams[selectedTeam][selectedSlot].assignedTeam = -1;
            }
            heroTeams[selectedTeam][selectedSlot] = null;
        }
        else
        {

            if (hero.assignedTeam != -1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (heroTeams[hero.assignedTeam][i] == hero)
                        heroTeams[hero.assignedTeam][i] = null;
                }
            }
            if (heroTeams[selectedTeam][selectedSlot] != null)
                heroTeams[selectedTeam][selectedSlot].assignedTeam = -1;
            heroTeams[selectedTeam][selectedSlot] = hero;
            hero.assignedTeam = selectedTeam;
        }
    }

    public bool AddEquipmentToInventory(Equipment newEquipment)
    {
        if (equipmentInventory.Contains(newEquipment))
            return false;
        equipmentInventory.Add(newEquipment);
        SaveManager.CurrentSave.SaveEquipmentData(newEquipment);
        return true;
    }

    public bool AddArchetypeToInventory(ArchetypeItem newArchetype)
    {
        if (archetypeInventory.Contains(newArchetype))
            return false;
        archetypeInventory.Add(newArchetype);
        SaveManager.CurrentSave.SaveArchetypeItemData(newArchetype);
        return true;
    }

    public bool AddAbilityToInventory(AbilityCoreItem newAbility)
    {
        if (abilityStorageInventory.Contains(newAbility))
            return false;
        abilityStorageInventory.Add(newAbility);
        SaveManager.CurrentSave.SaveAbilityCoreData(newAbility);
        return true;
    }

    public bool AddHeroToList(HeroData hero, bool saveData = true)
    {
        if (heroList.Contains(hero))
            return false;
        heroList.Add(hero);
        SaveManager.CurrentSave.SaveHeroData(hero);
        return true;
    }

    public bool RemoveEquipmentFromInventory(Equipment equip)
    {
        equipmentInventory.Remove(equip);
        SaveManager.CurrentSave.RemoveEquipmentData(equip);
        return true;
    }

    public bool RemoveArchetypeFromInventory(ArchetypeItem archetype)
    {
        archetypeInventory.Remove(archetype);
        SaveManager.CurrentSave.RemoveArchetypeItemData(archetype);
        return true;
    }

    public bool RemoveAbilityFromInventory(AbilityCoreItem newAbility)
    {
        abilityStorageInventory.Remove(newAbility);
        SaveManager.CurrentSave.RemoveAbilityCoreData(newAbility);
        return true;
    }

    public bool RemoveHeroFromList(HeroData hero)
    {
        heroList.Remove(hero);
        SaveManager.CurrentSave.RemoveHeroData(hero);
        return true;
    }

    public void ModifyExpStock(int value)
    {
        ExpStock += value;
        if (ExpStock < 0)
            ExpStock = 0;
        if (ExpStock > maxExpStock)
            ExpStock = maxExpStock;

        SaveManager.CurrentSave.SavePlayerData();
    }

    public void ModifyItemFragments(int value)
    {
        ItemFragments += value;
        if (ItemFragments < 0)
            ItemFragments = 0;
        if (ItemFragments > maxItemFragments)
            ItemFragments = maxItemFragments;

        SaveManager.CurrentSave.SavePlayerData();
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

        SaveManager.CurrentSave.SavePlayerData();
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

        if (stageClearInfo[stageId] > 0)
        {
            StageInfoBase stage = ResourceManager.Instance.GetStageInfo(stageId);
            StageInfoBase unlockedStage = ResourceManager.Instance.GetStageInfo(stage.requiredToUnlock);

            if (!stageClearInfo.ContainsKey(stage.requiredToUnlock))
                stageClearInfo.Add(stage.requiredToUnlock, 0);

            if (unlockedStage.act != stage.act)
            {
                if (!worldUnlockInfo.ContainsKey(unlockedStage.act) || !worldUnlockInfo[unlockedStage.act])
                {
                    worldUnlockInfo.Add(unlockedStage.act, true);
                }
            }
        }

        SaveManager.CurrentSave.SavePlayerData();
    }

    public bool IsStageUnlocked(string stageId)
    {
        if (!stageClearInfo.ContainsKey(stageId))
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

    public bool IsWorldUnlocked(int world)
    {
        if (!worldUnlockInfo.ContainsKey(world))
            return false;
        else
            return worldUnlockInfo[world];
    }
}

public enum ConsumableType
{
    AFFIX_REROLLER,
    AFFIX_CRAFTER
}