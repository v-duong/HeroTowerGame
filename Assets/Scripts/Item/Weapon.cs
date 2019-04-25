﻿using System.Collections.Generic;
using UnityEngine;
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
        for(int i = 0; i < (int)ElementType.COUNT;i++)
        {
            weaponDamage[(ElementType)i] = new MinMaxRange();
        }
        weaponDamage[ElementType.PHYSICAL].SetMinMax(e.minDamage, e.maxDamage);
        CriticalChance = e.criticalChance;
        AttackSpeed = e.attackSpeed;
        WeaponRange = e.weaponRange;
    }

    public override EquipmentType GetItemType()
    {
        return EquipmentType.WEAPON;
    }

    public override bool UpdateItemStats()
    {
        base.UpdateItemStats();
        Dictionary<BonusType, StatBonus> localBonusTotals = new Dictionary<BonusType, StatBonus>();
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        GetLocalModValues(localBonusTotals, affixes, global::EquipmentType.WEAPON);

        PhysicalDamage.min = CalculateStat(Base.minDamage, BonusType.LOCAL_PHYSICAL_DAMAGE_MIN, localBonusTotals);
        PhysicalDamage.max = CalculateStat(Base.maxDamage, BonusType.LOCAL_PHYSICAL_DAMAGE_MAX, localBonusTotals);
        PhysicalDamage.min = CalculateStat(PhysicalDamage.min, BonusType.LOCAL_PHYSICAL_DAMAGE, localBonusTotals);
        PhysicalDamage.max = CalculateStat(PhysicalDamage.max, BonusType.LOCAL_PHYSICAL_DAMAGE, localBonusTotals);

        weaponDamage[ElementType.FIRE].min = CalculateStat(0, BonusType.LOCAL_FIRE_DAMAGE_MIN, localBonusTotals);
        weaponDamage[ElementType.FIRE].max = CalculateStat(0, BonusType.LOCAL_FIRE_DAMAGE_MAX, localBonusTotals);

        weaponDamage[ElementType.COLD].min = CalculateStat(0, BonusType.LOCAL_COLD_DAMAGE_MIN, localBonusTotals);
        weaponDamage[ElementType.COLD].max = CalculateStat(0, BonusType.LOCAL_COLD_DAMAGE_MAX, localBonusTotals);

        weaponDamage[ElementType.LIGHTNING].min = CalculateStat(0, BonusType.LOCAL_LIGHTNING_DAMAGE_MIN, localBonusTotals);
        weaponDamage[ElementType.LIGHTNING].max = CalculateStat(0, BonusType.LOCAL_LIGHTNING_DAMAGE_MAX, localBonusTotals);

        weaponDamage[ElementType.EARTH].min = CalculateStat(0, BonusType.LOCAL_EARTH_DAMAGE_MIN, localBonusTotals);
        weaponDamage[ElementType.EARTH].max = CalculateStat(0, BonusType.LOCAL_EARTH_DAMAGE_MAX, localBonusTotals);

        weaponDamage[ElementType.DIVINE].min = CalculateStat(0, BonusType.LOCAL_DIVINE_DAMAGE_MIN, localBonusTotals);
        weaponDamage[ElementType.DIVINE].max = CalculateStat(0, BonusType.LOCAL_DIVINE_DAMAGE_MAX, localBonusTotals);

        weaponDamage[ElementType.VOID].min = CalculateStat(0, BonusType.LOCAL_VOID_DAMAGE_MIN, localBonusTotals);
        weaponDamage[ElementType.VOID].max = CalculateStat(0, BonusType.LOCAL_VOID_DAMAGE_MAX, localBonusTotals);

        CriticalChance = (float)CalculateStat(Base.criticalChance, BonusType.LOCAL_CRITICAL_CHANCE, localBonusTotals);
        AttackSpeed = (float)CalculateStat(Base.attackSpeed, BonusType.LOCAL_ATTACK_SPEED, localBonusTotals);

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
                tags.Add(GroupType.ONE_HANDED_WEAPON);
                tags.Add(GroupType.MELEE_WEAPON);
                break;
        }
        return tags;
    }
}