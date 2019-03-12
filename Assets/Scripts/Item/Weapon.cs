public class Weapon : Equipment
{
    public int maxDamage;
    public int minDamage;
    public float criticalChance;
    public float weaponRange;

    public Weapon (EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        maxDamage = e.maxDamage;
        minDamage = e.minDamage;
        criticalChance = e.criticalChance;
        weaponRange = e.weaponRange;
    }
}