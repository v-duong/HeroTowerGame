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
        GetLocalModValues(bonusTotals, GetAllAffixes(), ItemType.ARMOR);

        armor = CalculateStat(Base.armor, bonusTotals, BonusType.LOCAL_ARMOR);
        shield = CalculateStat(Base.shield, bonusTotals, BonusType.LOCAL_MAX_SHIELD);
        dodgeRating = CalculateStat(Base.dodgeRating, bonusTotals, BonusType.LOCAL_DODGE_RATING);
        resolveRating = CalculateStat(Base.resolveRating, bonusTotals, BonusType.LOCAL_RESOLVE_RATING);

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