using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public EquipmentBase Base { get { return ResourceManager.Instance.GetEquipmentBase(BaseId); } }
    private int BaseId { get; set; }
    public int armor;
    public int shield;
    public int dodgeRating;
    public int resolveRating;
    public float costModifier;
    public HeroActor equippedToHero;
    public List<Affix> innate;

    public Equipment(EquipmentBase e, int ilvl)
    {
        BaseId = e.id;
        Name = e.name;
        armor = e.armor;
        shield = e.shield;
        dodgeRating = e.dodgeRating;
        resolveRating = e.resolveRating;
        costModifier = e.sellValue;
        Rarity = RarityType.NORMAL;
        ItemLevel = ilvl;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
        innate = new List<Affix>();
        itemType = e.group;
        equippedToHero = null;
    }


}
