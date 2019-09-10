using System;
using System.Collections.Generic;

public abstract class Equipment : AffixedItem
{
    public static readonly int MaxLevelReq = 95;
    public EquipmentBase Base;
    public float costModifier;
    public int levelRequirement;
    public int strRequirement;
    public int intRequirement;
    public int agiRequirement;
    public int willRequirement;
    public HeroData equippedToHero;
    public List<Affix> innate;

    public bool IsEquipped
    {
        get
        {
            if (equippedToHero == null)
                return false;
            else
                return true;
        }
    }

    protected Equipment(EquipmentBase e, int ilvl)
    {
        Id = Guid.NewGuid();
        Base = e;
        Name = LocalizationManager.Instance.GetLocalizationText_Equipment(e.idName);
        costModifier = e.sellValue;
        strRequirement = e.strengthReq;
        intRequirement = e.intelligenceReq;
        agiRequirement = e.agilityReq;
        willRequirement = e.willReq;
        levelRequirement = Math.Min(e.dropLevel, MaxLevelReq);
        Rarity = RarityType.NORMAL;
        ItemLevel = ilvl;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
        innate = new List<Affix>();
        equippedToHero = null;

        if (e.hasInnate)
        {
            Affix newInnate = new Affix(ResourceManager.Instance.GetAffixBase(e.innateAffixId, AffixType.INNATE));
            innate.Add(newInnate);
        }
    }

    public static Equipment CreateEquipmentFromBase(EquipmentBase equipmentBase, int ilvl)
    {
        Equipment e;
        if (equipmentBase.equipSlot == EquipSlotType.WEAPON)
        {
            e = new Weapon(equipmentBase, ilvl);
        }
        else if ((int)equipmentBase.equipSlot >= 6)
        {
            e = new Accessory(equipmentBase, ilvl);
        }
        else
        {
            e = new Armor(equipmentBase, ilvl);
        }
        return e;
    }

    public static Equipment CreateEquipmentFromBase(string equipmentString, int ilvl)
    {
        EquipmentBase equipmentBase = ResourceManager.Instance.GetEquipmentBase(equipmentString);
        if (equipmentBase == null)
            return null;
        return CreateEquipmentFromBase(equipmentBase, ilvl);
    }

    public static Equipment CreateRandomEquipment(int ilvl, GroupType? group = null)
    {
        return CreateEquipmentFromBase(ResourceManager.Instance.GetRandomEquipmentBase(ilvl, group), ilvl);
    }

    public override bool UpgradeRarity()
    {
        if (Rarity == RarityType.EPIC)
            return false;
        else
            Rarity++;
        UpdateName();
        AddRandomAffix();
        return true;
    }

    public override void UpdateName()
    {
        if (Rarity == RarityType.RARE || Rarity == RarityType.EPIC)
        {
            Name = LocalizationManager.Instance.GenerateRandomItemName(GetGroupTypes());
        }
        else if (Rarity == RarityType.UNCOMMON)
        {
        }
        else
        {
            Name = LocalizationManager.Instance.GetLocalizationText_Equipment(Base.idName);
        }
    }

    public override bool UpdateItemStats()
    {
        int req = Base.dropLevel;

        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);

        foreach (Affix affix in affixes)
        {
            if (affix.Base.spawnLevel > req)
                req = affix.Base.spawnLevel;
        }

        levelRequirement = Math.Min(req, MaxLevelReq);

        return true;
    }

    public override List<Affix> GetAllAffixes()
    {
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        affixes.AddRange(innate);
        return affixes;
    }

    protected static void GetLocalModValues(Dictionary<BonusType, StatBonus> dic, List<Affix> affixes, ItemType itemType)
    {
        int startValue = 0;
        switch (itemType)
        {
            case ItemType.ARMOR:
                startValue = Armor.LocalBonusStart;
                break;

            case ItemType.WEAPON:
                startValue = Weapon.LocalBonusStart;
                break;

            default:
                return;
        }

        foreach (Affix affix in affixes)
        {
            for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
            {
                AffixBonusProperty prop = affix.Base.affixBonuses[i];
                if ((int)prop.bonusType >= startValue && (int)prop.bonusType < startValue + 0x100)
                {
                    if (!dic.ContainsKey(prop.bonusType))
                        dic.Add(prop.bonusType, new StatBonus());
                    dic[prop.bonusType].AddBonus(prop.modifyType, affix.GetAffixValue(i));
                }
            }
        }
    }

    protected static int CalculateStat(int stat, BonusType bonusType, Dictionary<BonusType, StatBonus> dic)
    {
        if (dic.TryGetValue(bonusType, out StatBonus bonus))
        {
            return bonus.CalculateStat(stat);
        }
        else
        {
            return stat;
        }
    }

    protected static float CalculateStat(float stat, BonusType bonusType, Dictionary<BonusType, StatBonus> dic)
    {
        if (dic.TryGetValue(bonusType, out StatBonus bonus))
        {
            return bonus.CalculateStat(stat);
        }
        else
        {
            return stat;
        }
    }
}