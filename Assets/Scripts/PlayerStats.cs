using System;
using System.Collections.Generic;

public class PlayerStats
{
    public readonly static int maxEquipInventory = 250;
    public readonly static int maxArchetypeInventory = 100;
    public readonly static int maxAbilityInventory = 100;
    public readonly static int maxHeroes = 100;
    public readonly static int maxExpStock = 2000000;
    public readonly static int maxItemFragments = 500000;
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
        heroList = new List<HeroData>();
        heroTeams = new List<HeroData[]>();
        for (int i = 0; i < 5; i++)
        {
            heroTeams.Add(new HeroData[5]);
        }
    }

    public Equipment GetEquipmentByGuid(Guid id)
    {
        return equipmentInventory.Find(x => x.Id == id);
    }

    public bool AddEquipmentToInventory(Equipment newEquipment)
    {
        equipmentInventory.Add(newEquipment);
        return true;
    }

    public bool AddArchetypeToInventory(ArchetypeItem newArchetype)
    {
        archetypeInventory.Add(newArchetype);
        return true;
    }

    public bool AddAbilityToInventory(AbilityCoreItem newAbility)
    {
        abilityStorageInventory.Add(newAbility);
        return true;
    }

    public bool AddHeroToList(HeroData hero)
    {
        heroList.Add(hero);
        UIManager.Instance.HeroScrollContent.AddHeroSlot(hero);
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

    public void ModifyArchetypeFragments(int value)
    {
        ArchetypeFragments += value;
        if (ArchetypeFragments < 0)
            ArchetypeFragments = 0;
        if (ArchetypeFragments > maxArchetypeFragments)
            ArchetypeFragments = maxArchetypeFragments;
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
}

public enum ConsumableType
{
    LOW_TIER_UPGRADER,
    RARE_TO_EPIC,
    AFFIX_REROLLER,
    AFFIX_CRAFTER
}