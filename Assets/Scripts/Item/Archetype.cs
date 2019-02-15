using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archetype : Item
{
    public int strength;
    public int intelligence;
    public int agility;
    public int will;

    public int archetypeLevel;

    public List<Affix> enchantments;


    public Archetype()
    {
        ItemType = GroupType.ARCHETYPE;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
        enchantments = new List<Affix>();
    }
}
