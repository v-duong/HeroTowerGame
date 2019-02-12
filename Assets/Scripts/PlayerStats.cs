using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public readonly static int maxEquipInventory = 500;
    public readonly static int maxArchetypeInventory = 100;

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

    public PlayerStats Initialize()
    {
        PlayerStats playerStats = new PlayerStats
        {
            catalyst_NormaltoMagic = 0,
            catalyst_MagicReroll = 0,
            catalyst_NormalToRare = 0,
            catalyst_MagicToRare = 0,
            catalyst_RareReroll = 0,
            catalyst_RareToEpic = 0,
            catalyst_AddAffix = 0,
            catalyst_RemoveAffix = 0,
            catalyst_ToNormal = 0,
            catalyst_ValueReroll = 0,
            equipmentInventory = new List<Equipment>(),
            archetypeInventory = new List<Archetype>()
        };
        return playerStats;
    }


}
