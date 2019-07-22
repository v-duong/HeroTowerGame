﻿using System;
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

    public List<Equipment> equipmentInventory;
    public List<ArchetypeItem> archetypeInventory;
    public List<HeroData> heroList;
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

    public void AddEquipmentToInventory(Equipment newEquipment)
    {
        equipmentInventory.Add(newEquipment);
    }

    public void AddArchetypeToInventory(ArchetypeItem newArchetype)
    {
        archetypeInventory.Add(newArchetype);
    }

    public void AddHeroToList(HeroData hero)
    {
        heroList.Add(hero);
        UIManager.Instance.HeroScrollContent.AddHeroSlot(hero);
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