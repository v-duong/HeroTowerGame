using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public EquipmentBase Base { get { return ResourceManager.Instance.GetEquipmentBase(BaseId); } }
    public int BaseId { get; private set; }
    public int armor;
    public int shield;
    public int dodge;
    public int magicDodge;
    public float regen;
    public bool isEquipped;
    public Affix innate;

    public Equipment(EquipmentBase e, int ilvl)
    {
        BaseId = e.id;
        Name = e.name;
        armor = e.armor;
        shield = e.shield;
        dodge = e.dodge;
        magicDodge = e.magicDodge;
        regen = e.regen;
        Rarity = RarityType.NORMAL;
        ItemLevel = ilvl;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
    }

}
