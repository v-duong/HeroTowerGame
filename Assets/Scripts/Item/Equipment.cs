using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public EquipmentBase Base;
    public int armor;
    public int shield;
    public int dodge;
    public int magicDodge;
    public float regen;
    public bool isEquipped;
    public Affix innate;

    public Equipment(EquipmentBase e, int ilvl)
    {
        Base = e;
        name = e.name;
        armor = e.armor;
        shield = e.shield;
        dodge = e.dodge;
        magicDodge = e.magicDodge;
        regen = e.regen;
        rarity = RarityType.NORMAL;
        itemLevel = ilvl;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
    }

}
