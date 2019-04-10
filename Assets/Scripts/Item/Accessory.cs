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
        return true;
    }

    public override HashSet<GroupType> GetGroupTypes()
    {
        HashSet<GroupType> tags = new HashSet<GroupType>();
        tags.Add(GroupType.ALL_ACCESSORY);
        tags.Add(Base.group);
        return tags;
    }
}