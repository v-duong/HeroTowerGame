using System;
using System.Collections.Generic;

public class Weapon : Equipment
{
    public const int LocalBonusStart = 0x800;
    public MinMaxRange physicalDamage;
    public Dictionary<ElementType, MinMaxRange> nonPhysicalDamage;
    public float criticalChance;
    public float weaponRange;
    public float attackSpeed;

    public Weapon(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        physicalDamage = new MinMaxRange();
        nonPhysicalDamage = new Dictionary<ElementType, MinMaxRange>();
        physicalDamage.SetMinMax(e.minDamage, e.maxDamage);
        criticalChance = e.criticalChance;
        attackSpeed = e.attackSpeed;
        weaponRange = e.weaponRange;
    }

    public override ItemType GetItemType()
    {
        return ItemType.WEAPON;
    }

    public override bool UpdateItemStats()
    {
        Dictionary<BonusType, HeroStatBonus> bonusTotals = new Dictionary<BonusType, HeroStatBonus>();
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        GetLocalModValues(bonusTotals, affixes, ItemType.WEAPON);

        physicalDamage.min = CalculateStat(Base.minDamage, BonusType.LOCAL_PHYSICAL_DAMAGE_MIN, bonusTotals);
        physicalDamage.max = CalculateStat(Base.maxDamage, BonusType.LOCAL_PHYSICAL_DAMAGE_MAX, bonusTotals);
        physicalDamage.min = CalculateStat(physicalDamage.min, BonusType.LOCAL_PHYSICAL_DAMAGE, bonusTotals);
        physicalDamage.max = CalculateStat(physicalDamage.max, BonusType.LOCAL_PHYSICAL_DAMAGE, bonusTotals);
        criticalChance = (float)CalculateStat(Base.criticalChance, BonusType.LOCAL_CRITICAL_CHANCE, bonusTotals);
        attackSpeed = (float)CalculateStat(Base.attackSpeed, BonusType.LOCAL_ATTACK_SPEED, bonusTotals);

        return true;
    }

    private void ResetDamageValues()
    {
    }

    public override HashSet<GroupType> GetGroupTypes()
    {
        HashSet<GroupType> tags = new HashSet<GroupType>();
        tags.Add(GroupType.WEAPON);
        tags.Add(Base.group);
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
