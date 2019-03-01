using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public readonly static int maxEquipInventory = 500;
    public readonly static int maxArchetypeInventory = 100;
    public readonly static int maxAbilityInventory = 200;

    public int gold;
    public int archetypeFragments;
    public int expStock;

    public int catalyst_NormaltoMagic; 
    public int catalyst_MagicReroll; 
    public int catalyst_NormalToRare;
    public int catalyst_MagicToRare;
    public int catalyst_RareReroll;
    public int catalyst_RareToEpic;
    public int catalyst_AddAffix;
    public int catalyst_RemoveAffix;
    public int catalyst_ToNormal;
    public int catalyst_ValueReroll;

    public List<Equipment> equipmentInventory;
    public List<Archetype> archetypeInventory;
    public List<HeroActor> heroList;

    public PlayerStats()
    {
        gold = 0;
        archetypeFragments = 0;
        expStock = 0;
        catalyst_NormaltoMagic = 0;
        catalyst_MagicReroll = 0;
        catalyst_NormalToRare = 0;
        catalyst_MagicToRare = 0;
        catalyst_RareReroll = 0;
        catalyst_RareToEpic = 0;
        catalyst_AddAffix = 0;
        catalyst_RemoveAffix = 0;
        catalyst_ToNormal = 0;
        catalyst_ValueReroll = 0;
        equipmentInventory = new List<Equipment>();
        archetypeInventory = new List<Archetype>();
    }

    public void AddEquipmentToInventory(Equipment newEquipment)
    {
        equipmentInventory.Add(newEquipment);
        UIManager.Instance.AddEquipmentSlot(newEquipment);
    }
}
