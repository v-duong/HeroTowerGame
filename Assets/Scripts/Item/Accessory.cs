using System;
using System.Collections.Generic;

public class Accessory : Equipment
{

    public Accessory(EquipmentBase e, int ilvl) : base(e, ilvl)
    {

    }

    public override ItemType GetItemType()
    {
        return ItemType.ACCESSORY;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        return true;
    }

    public override HashSet<GroupType> GetGroupTypes()
    {
        HashSet<GroupType> tags = new HashSet<GroupType>
        {
            GroupType.ALL_ACCESSORY,
            Base.group
        };
        return tags;
    }
}