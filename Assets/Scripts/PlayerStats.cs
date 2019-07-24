using System;
using System.Collections.Generic;

public class PlayerStats
{
    public readonly static int maxEquipInventory = 250;
    public readonly static int maxArchetypeInventory = 100;
    public readonly static int maxAbilityInventory = 100;
    public readonly static int maxHeroes = 100;

    public int gold;
    public int archetypeFragments;
    public int expStock;

    public Dictionary<ConsumableType, int> consumables;

    private List<Equipment> equipmentInventory;
    private List<ArchetypeItem> archetypeInventory;
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
        gold = 0;
        archetypeFragments = 0;
        expStock = 0;
        consumables = new Dictionary<ConsumableType, int>();
        foreach (ConsumableType c in Enum.GetValues(typeof(ConsumableType)))
        {
            consumables.Add(c, 0);
        }
        equipmentInventory = new List<Equipment>();
        archetypeInventory = new List<ArchetypeItem>();
        heroList = new List<HeroData>();
        heroTeams = new List<HeroData[]>();
        for (int i = 0; i < 5; i++)
        {
            heroTeams.Add(new HeroData[5]);
        }
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

    public bool RemoveHeroFromList(HeroData hero)
    {
        heroList.Remove(hero);
        return true;
    }
}

public enum ConsumableType
{
    NORMAL_TO_MAGIC,
    MAGIC_REROLL,
    NORMAL_TO_RARE,
    MAGIC_TO_RARE,
    RARE_REROLL,
    RARE_TO_EPIC,
    ADD_AFFIX,
    REMOVE_AFFIX,
    RESET_NORMAL,
    VALUE_REROLL,
    ENCHANTMENT_EXTENSION,
}