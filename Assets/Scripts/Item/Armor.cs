using System.Collections.Generic;
using System;
using UnityEngine;

public class Armor : Equipment
{
    public const int LocalBonusStart = 0x700;
    public int armor;
    public int shield;
    public int dodgeRating;
    public int resolveRating;
    

    public Armor(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        armor = e.armor;
        shield = e.shield;
        dodgeRating = e.dodgeRating;
        resolveRating = e.resolveRating;
    }

    public override ItemType GetItemType()
    {
        return ItemType.ARMOR;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        Dictionary<BonusType, StatBonus> bonusTotals = new Dictionary<BonusType, StatBonus>();
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        GetLocalModValues(bonusTotals, affixes, global::ItemType.ARMOR);

        armor = CalculateStat(Base.armor, BonusType.LOCAL_ARMOR, bonusTotals);
        shield = CalculateStat(Base.shield, BonusType.LOCAL_MAX_SHIELD, bonusTotals);
        dodgeRating = CalculateStat(Base.dodgeRating, BonusType.LOCAL_DODGE_RATING, bonusTotals);
        resolveRating = CalculateStat(Base.resolveRating, BonusType.LOCAL_RESOLVE_RATING, bonusTotals);

        return true;
    }

    public override HashSet<GroupType> GetGroupTypes()
    {
        HashSet<GroupType> tags = new HashSet<GroupType>
        {
            GroupType.ALL_ARMOR,
            Base.group
        };
        switch (Base.equipSlot)
        {
            case EquipSlotType.BODY_ARMOR:
                tags.Add(GroupType.BODY_ARMOR);
                break;
        }
        return tags;
    }
}