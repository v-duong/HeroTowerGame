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
            Base.group
        };
        switch (Base.equipSlot)
        {
            case EquipSlotType.BODY_ARMOR:
                tags.Add(GroupType.BODY_ARMOR);
                tags.Add(GroupType.ALL_ARMOR);
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
                        tags.Add(GroupType.ALL_ARMOR);
                        if (Enum.TryParse(Base.group.ToString().Replace("SHIELD", "ARMOR"), out GroupType derived))
                            tags.Add(derived);
                        break;
                    default:
                        break;
                }
                break;

            case EquipSlotType.HEADGEAR:
                tags.Add(GroupType.HEADGEAR);
                tags.Add(GroupType.ALL_ARMOR);
                break;

            case EquipSlotType.GLOVES:
                tags.Add(GroupType.GLOVES);
                tags.Add(GroupType.ALL_ARMOR);
                break;

            case EquipSlotType.BOOTS:
                tags.Add(GroupType.BOOTS);
                tags.Add(GroupType.ALL_ARMOR);
                break;

            case EquipSlotType.BELT:
                tags.Add(GroupType.BELT);
                tags.Add(GroupType.ALL_ACCESSORY);
                break;

            case EquipSlotType.NECKLACE:
                tags.Add(GroupType.NECKLACE);
                tags.Add(GroupType.ALL_ACCESSORY);
                break;

            case EquipSlotType.RING_SLOT_1:
            case EquipSlotType.RING_SLOT_2:
            case EquipSlotType.RING:
                tags.Add(GroupType.RING);
                tags.Add(GroupType.ALL_ACCESSORY);
                break;
        }
        return tags;
    }
}