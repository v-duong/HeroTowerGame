using System;
using System.Collections.Generic;

public class Weapon : Equipment
{
    public const int LocalBonusStart = 0x800;
    private Dictionary<ElementType, MinMaxRange> weaponDamage;
    public float CriticalChance { get; private set; }
    public float WeaponRange { get; private set; }
    public float AttackSpeed { get; private set; }

    public MinMaxRange PhysicalDamage
    {
        get
        {
            return weaponDamage[0];
        }
    }

    public Weapon(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        weaponDamage = new Dictionary<ElementType, MinMaxRange>();
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            weaponDamage[element] = new MinMaxRange();
        }
        weaponDamage[ElementType.PHYSICAL].SetMinMax(e.minDamage, e.maxDamage);
        CriticalChance = e.criticalChance;
        AttackSpeed = e.attackSpeed;
        WeaponRange = e.weaponRange;
    }

    public override ItemType GetItemType()
    {
        return ItemType.WEAPON;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        Dictionary<BonusType, StatBonus> localBonusTotals = new Dictionary<BonusType, StatBonus>();
        GetLocalModValues(localBonusTotals, GetAllAffixes(), ItemType.WEAPON);

        PhysicalDamage.min = CalculateStat(Base.minDamage, localBonusTotals, BonusType.LOCAL_PHYSICAL_DAMAGE_MIN);
        PhysicalDamage.max = CalculateStat(Base.maxDamage, localBonusTotals, BonusType.LOCAL_PHYSICAL_DAMAGE_MAX);
        PhysicalDamage.min = CalculateStat(PhysicalDamage.min, localBonusTotals, BonusType.LOCAL_PHYSICAL_DAMAGE);
        PhysicalDamage.max = CalculateStat(PhysicalDamage.max, localBonusTotals, BonusType.LOCAL_PHYSICAL_DAMAGE);

        weaponDamage[ElementType.FIRE].min = CalculateStat(0, localBonusTotals, BonusType.LOCAL_FIRE_DAMAGE_MIN);
        weaponDamage[ElementType.FIRE].max = CalculateStat(0, localBonusTotals, BonusType.LOCAL_FIRE_DAMAGE_MAX);
        weaponDamage[ElementType.FIRE].min = CalculateStat(weaponDamage[ElementType.FIRE].min, localBonusTotals, BonusType.LOCAL_FIRE_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);
        weaponDamage[ElementType.FIRE].max = CalculateStat(weaponDamage[ElementType.FIRE].max, localBonusTotals, BonusType.LOCAL_FIRE_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);

        weaponDamage[ElementType.COLD].min = CalculateStat(0, localBonusTotals, BonusType.LOCAL_COLD_DAMAGE_MIN);
        weaponDamage[ElementType.COLD].max = CalculateStat(0, localBonusTotals, BonusType.LOCAL_COLD_DAMAGE_MAX);
        weaponDamage[ElementType.COLD].min = CalculateStat(weaponDamage[ElementType.COLD].min, localBonusTotals, BonusType.LOCAL_COLD_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);
        weaponDamage[ElementType.COLD].max = CalculateStat(weaponDamage[ElementType.COLD].max, localBonusTotals, BonusType.LOCAL_COLD_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);

        weaponDamage[ElementType.LIGHTNING].min = CalculateStat(0, localBonusTotals, BonusType.LOCAL_LIGHTNING_DAMAGE_MIN);
        weaponDamage[ElementType.LIGHTNING].max = CalculateStat(0, localBonusTotals, BonusType.LOCAL_LIGHTNING_DAMAGE_MAX);
        weaponDamage[ElementType.LIGHTNING].min = CalculateStat(weaponDamage[ElementType.LIGHTNING].min, localBonusTotals, BonusType.LOCAL_LIGHTNING_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);
        weaponDamage[ElementType.LIGHTNING].max = CalculateStat(weaponDamage[ElementType.LIGHTNING].max, localBonusTotals, BonusType.LOCAL_LIGHTNING_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);

        weaponDamage[ElementType.EARTH].min = CalculateStat(0, localBonusTotals, BonusType.LOCAL_EARTH_DAMAGE_MIN);
        weaponDamage[ElementType.EARTH].max = CalculateStat(0, localBonusTotals, BonusType.LOCAL_EARTH_DAMAGE_MAX);
        weaponDamage[ElementType.EARTH].min = CalculateStat(weaponDamage[ElementType.EARTH].min, localBonusTotals, BonusType.LOCAL_EARTH_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);
        weaponDamage[ElementType.EARTH].max = CalculateStat(weaponDamage[ElementType.EARTH].max, localBonusTotals, BonusType.LOCAL_EARTH_DAMAGE, BonusType.LOCAL_ELEMENTAL_DAMAGE);

        weaponDamage[ElementType.DIVINE].min = CalculateStat(0, localBonusTotals, BonusType.LOCAL_DIVINE_DAMAGE_MIN);
        weaponDamage[ElementType.DIVINE].max = CalculateStat(0, localBonusTotals, BonusType.LOCAL_DIVINE_DAMAGE_MAX);
        weaponDamage[ElementType.DIVINE].min = CalculateStat(weaponDamage[ElementType.DIVINE].min, localBonusTotals, BonusType.LOCAL_DIVINE_DAMAGE, BonusType.LOCAL_PRIMORDIAL_DAMAGE);
        weaponDamage[ElementType.DIVINE].max = CalculateStat(weaponDamage[ElementType.DIVINE].max, localBonusTotals, BonusType.LOCAL_DIVINE_DAMAGE, BonusType.LOCAL_PRIMORDIAL_DAMAGE);

        weaponDamage[ElementType.VOID].min = CalculateStat(0, localBonusTotals, BonusType.LOCAL_VOID_DAMAGE_MIN);
        weaponDamage[ElementType.VOID].max = CalculateStat(0, localBonusTotals, BonusType.LOCAL_VOID_DAMAGE_MAX);
        weaponDamage[ElementType.VOID].min = CalculateStat(weaponDamage[ElementType.VOID].min, localBonusTotals, BonusType.LOCAL_VOID_DAMAGE, BonusType.LOCAL_PRIMORDIAL_DAMAGE);
        weaponDamage[ElementType.VOID].max = CalculateStat(weaponDamage[ElementType.VOID].max, localBonusTotals, BonusType.LOCAL_VOID_DAMAGE, BonusType.LOCAL_PRIMORDIAL_DAMAGE);

        CriticalChance = CalculateStat(Base.criticalChance, localBonusTotals, BonusType.LOCAL_CRITICAL_CHANCE);
        AttackSpeed = CalculateStat(Base.attackSpeed, localBonusTotals, BonusType.LOCAL_ATTACK_SPEED);

        return true;
    }

    private void ResetDamageValues()
    {
    }

    public MinMaxRange GetWeaponDamage(ElementType e)
    {
        if (weaponDamage.TryGetValue(e, out MinMaxRange range))
            return range;
        else
            return new MinMaxRange();
    }

    public override HashSet<GroupType> GetGroupTypes()
    {
        HashSet<GroupType> tags = new HashSet<GroupType>
        {
            GroupType.WEAPON,
            Base.group
        };
        switch (Base.group)
        {
            case GroupType.ONE_HANDED_AXE:
            case GroupType.ONE_HANDED_SWORD:
            case GroupType.ONE_HANDED_MACE:
            case GroupType.FIST:
                tags.Add(GroupType.ATTACK_WEAPON);
                tags.Add(GroupType.MELEE_WEAPON);
                tags.Add(GroupType.ONE_HANDED_WEAPON);
                break;

            case GroupType.TWO_HANDED_AXE:
            case GroupType.TWO_HANDED_SWORD:
            case GroupType.TWO_HANDED_MACE:
            case GroupType.SPEAR:
                tags.Add(GroupType.MELEE_WEAPON);
                tags.Add(GroupType.ATTACK_WEAPON);
                tags.Add(GroupType.TWO_HANDED_WEAPON);
                break;

            case GroupType.BOW:
            case GroupType.CROSSBOW:
            case GroupType.TWO_HANDED_GUN:
                tags.Add(GroupType.RANGED_WEAPON);
                tags.Add(GroupType.ATTACK_WEAPON);
                tags.Add(GroupType.TWO_HANDED_WEAPON);
                break;

            case GroupType.ONE_HANDED_GUN:
                tags.Add(GroupType.RANGED_WEAPON);
                tags.Add(GroupType.ATTACK_WEAPON);
                tags.Add(GroupType.ONE_HANDED_WEAPON);
                break;

            case GroupType.WAND:
                tags.Add(GroupType.CASTER_WEAPON);
                tags.Add(GroupType.RANGED_WEAPON);
                tags.Add(GroupType.ONE_HANDED_WEAPON);
                break;

            case GroupType.STAFF:
                tags.Add(GroupType.CASTER_WEAPON);
                tags.Add(GroupType.ATTACK_WEAPON);
                tags.Add(GroupType.MELEE_WEAPON);
                tags.Add(GroupType.TWO_HANDED_WEAPON);
                break;
        }
        switch (Base.group)
        {
            case GroupType.ONE_HANDED_AXE:
            case GroupType.TWO_HANDED_AXE:
                tags.Add(GroupType.AXE_TYPE);
                break;

            case GroupType.ONE_HANDED_SWORD:
            case GroupType.TWO_HANDED_SWORD:
                tags.Add(GroupType.SWORD_TYPE);
                break;

            case GroupType.ONE_HANDED_MACE:
            case GroupType.TWO_HANDED_MACE:
                tags.Add(GroupType.MACE_TYPE);
                break;

            case GroupType.SPEAR:
            case GroupType.STAFF:
                tags.Add(GroupType.POLE_TYPE);
                break;

            case GroupType.TWO_HANDED_GUN:
            case GroupType.ONE_HANDED_GUN:
                tags.Add(GroupType.GUN_TYPE);
                break;

            case GroupType.BOW:
                tags.Add(GroupType.BOW_TYPE);
                break;
        }

        return tags;
    }

    public float GetPhysicalDPS()
    {
        float dps = (PhysicalDamage.min + PhysicalDamage.max) / 2f * AttackSpeed;
        return dps;
    }

    public float GetElementalDPS()
    {
        int min = weaponDamage[ElementType.FIRE].min + weaponDamage[ElementType.COLD].min + weaponDamage[ElementType.LIGHTNING].min + weaponDamage[ElementType.EARTH].min;
        int max = weaponDamage[ElementType.FIRE].max + weaponDamage[ElementType.COLD].max + weaponDamage[ElementType.LIGHTNING].max + weaponDamage[ElementType.EARTH].max;
        float dps = (min + max) / 2f * AttackSpeed;
        return dps;
    }

    public float GetPrimordialDPS()
    {
        int min = weaponDamage[ElementType.DIVINE].min + weaponDamage[ElementType.VOID].min;
        int max = weaponDamage[ElementType.DIVINE].max + weaponDamage[ElementType.VOID].max;
        float dps = (min + max) / 2f * AttackSpeed;
        return dps;
    }
}