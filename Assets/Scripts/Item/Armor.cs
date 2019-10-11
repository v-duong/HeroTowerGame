using System;
using System.Collections.Generic;

public class Armor : Equipment
{
    public const int LocalBonusStart = 0x700;
    public int armor;
    public int shield;
    public int dodgeRating;
    public int resolveRating;
    public int blockChance;
    public int blockProtection;

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
        blockChance = (int)CalculateStat(Base.criticalChance, bonusTotals, BonusType.LOCAL_BLOCK_CHANCE);
        blockProtection = (int)CalculateStat(Base.attackSpeed, bonusTotals, BonusType.LOCAL_BLOCK_PROTECTION);

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

            case EquipSlotType.WEAPON:
                break;

            case EquipSlotType.OFF_HAND:
                switch (Base.group)
                {
                    case GroupType.STR_SHIELD:
                    case GroupType.AGI_SHIELD:
                    case GroupType.INT_SHIELD:
                    case GroupType.WILL_SHIELD:
                    case GroupType.STR_INT_SHIELD:
                    case GroupType.STR_AGI_SHIELD:
                    case GroupType.STR_WILL_SHIELD:
                    case GroupType.INT_AGI_SHIELD:
                    case GroupType.INT_WILL_SHIELD:
                    case GroupType.AGI_WILL_SHIELD:
                        tags.Add(GroupType.SHIELD);
                        if (Enum.TryParse(Base.group.ToString().Replace("SHIELD", "ARMOR"), out GroupType derived))
                            tags.Add(derived);
                        break;
                    default:
                        break;
                }
                break;

            case EquipSlotType.HEADGEAR:
                break;

            case EquipSlotType.GLOVES:
                break;

            case EquipSlotType.BOOTS:
                break;

            case EquipSlotType.BELT:
                break;

            case EquipSlotType.NECKLACE:
                break;

            case EquipSlotType.RING_SLOT_1:
                break;

            case EquipSlotType.RING_SLOT_2:
                break;

            case EquipSlotType.RING:
                break;
        }
        return tags;
    }
}