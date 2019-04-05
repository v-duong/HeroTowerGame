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
}