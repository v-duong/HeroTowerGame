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

    public static Equipment CreateEquipmentFromBase(string equipmentString, int ilvl, RarityType rarity = RarityType.NORMAL)
    {
        EquipmentBase equipmentBase = ResourceManager.Instance.GetEquipmentBase(equipmentString);
        if (equipmentBase == null)
            return null;

        Equipment equip = CreateEquipmentFromBase(equipmentBase, ilvl);

        if (rarity > RarityType.NORMAL)
        {
            equip.Rarity = rarity;
            equip.RerollAffixesAtRarity();
        }

        return equip;
    }

    public static Equipment CreateRandomEquipment(int ilvl, GroupType? group = null, RarityType rarity = RarityType.NORMAL)
    {
        Equipment equip = CreateEquipmentFromBase(ResourceManager.Instance.GetRandomEquipmentBase(ilvl, group), ilvl);
        if (rarity > RarityType.NORMAL)
        {
            equip.Rarity = rarity;
            equip.RerollAffixesAtRarity();
        }
        return equip;
    }

    public static Equipment CreateRandomUnique(int ilvl, GroupType? group = null)
    {
        return CreateUniqueFromBase(ResourceManager.Instance.GetRandomUniqueBase(ilvl, group), ilvl);
    }

    public static Equipment CreateUniqueFromBase(UniqueBase uniqueBase, int ilvl)
    {
        Equipment e = CreateEquipmentFromBase(uniqueBase, ilvl);
        e.Rarity = RarityType.UNIQUE;
        foreach (AffixBase affixBase in uniqueBase.fixedUniqueAffixes)
        {
            e.prefixes.Add(new Affix(affixBase));
        }
        e.UpdateItemStats();
        return e;
    }

    public static Equipment CreateUniqueFromBase(string uniqueId, int ilvl, int uniqueVersion)
    {
        UniqueBase uniqueBase = ResourceManager.Instance.GetUniqueBase(uniqueId);
        if (uniqueBase == null)
            return null;
        if (uniqueVersion == uniqueBase.uniqueVersion)
        {
            Equipment e = CreateEquipmentFromBase(uniqueBase, ilvl);
            e.Rarity = RarityType.UNIQUE;
            return e;
        }
        else
        {
            return CreateUniqueFromBase(uniqueBase, ilvl);
        }
    }

    public override bool UpgradeRarity()
    {
        if (Rarity < RarityType.EPIC)
            Rarity++;
        else
            return false;
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

    public override int GetItemValue()
    {
        float affixValueMultiplier = 1;
        float rollValue;
        int actualAffixCount;
        if (Rarity != RarityType.UNIQUE)
        {
            foreach (Affix affix in GetAllAffixes())
            {
                if (affix.AffixType == AffixType.INNATE)
                    continue;
                int weightValue = 0;
                foreach (var weight in affix.Base.spawnWeight)
                {
                    if (GetGroupTypes().Contains(weight.type))
                    {
                        weightValue = weight.weight;
                        break;
                    }
                }

                float weightModifier;
                if (weightValue > 0)
                {
                    weightModifier = 1000f / weightValue;
                }
                else
                {
                    weightModifier = 5f;
                }

                affixValueMultiplier += weightModifier;

                if (affix.Base.affixBonuses.Count == 0)
                    continue;
                rollValue = 0;

                for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
                {
                    //UnityEngine.Debug.Log(affix.Base.idName + " " + +rollValueMultiplier + " " + affix.GetAffixValue(i) + " " + affix.Base.affixBonuses[i].minValue);
                    float valueDifference = affix.Base.affixBonuses[i].maxValue - affix.Base.affixBonuses[i].minValue;
                    if (valueDifference == 0)
                    {
                        rollValue += 0.7f;
                        continue;
                    }
                    rollValue += (affix.GetAffixValue(i) - affix.Base.affixBonuses[i].minValue) / valueDifference * 0.7f;
                }
                rollValue /= affix.Base.affixBonuses.Count;
                affixValueMultiplier += rollValue;
            }
        }
        else
        {
            UniqueBase uniqueBase = Base as UniqueBase;
            float rollValueMultiplier = 0f;
            actualAffixCount = 0;
            foreach (Affix affix in GetAllAffixes())
            {
                if (affix.Base.affixBonuses.Count == 0)
                    continue;
                rollValue = 0;
                for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
                {
                    //UnityEngine.Debug.Log(affix.Base.idName + " " + +rollValueMultiplier + " " + affix.GetAffixValue(i) + " " + affix.Base.affixBonuses[i].minValue);
                    float valueDifference = affix.Base.affixBonuses[i].maxValue - affix.Base.affixBonuses[i].minValue;
                    if (valueDifference == 0)
                    {
                        rollValue += 0.5f;
                        continue;
                    }
                    rollValue += (affix.GetAffixValue(i) - affix.Base.affixBonuses[i].minValue) / valueDifference;
                }
                rollValueMultiplier += rollValue / affix.Base.affixBonuses.Count;
                actualAffixCount++;
            }
            rollValueMultiplier = 1 + rollValueMultiplier / actualAffixCount;

            if (uniqueBase.spawnWeight == 0)
                affixValueMultiplier = 500f * rollValueMultiplier;
            else
                affixValueMultiplier = Math.Min((1000f / uniqueBase.spawnWeight) * (uniqueBase.dropLevel / 100f) * 50f * rollValueMultiplier, 500f);
        }

        return Math.Max((int)(affixValueMultiplier * ItemLevel * 1.5f), 1);
    }

    protected static void GetLocalModValues(Dictionary<BonusType, StatBonus> dic, List<Affix> affixes, ItemType itemType)
    {
        int startValue;
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

    protected static int CalculateStat(int stat, Dictionary<BonusType, StatBonus> dic, params BonusType[] bonusTypes)
    {
        return (int)CalculateStat((float)stat, dic, bonusTypes);
    }

    protected static float CalculateStat(float stat, Dictionary<BonusType, StatBonus> dic, params BonusType[] bonusTypes)
    {
        StatBonus totalBonus = new StatBonus();
        foreach (BonusType bonusType in bonusTypes)
        {
            if (dic.TryGetValue(bonusType, out StatBonus bonus))
            {
                totalBonus.AddBonuses(bonus);
            }
        }
        return totalBonus.CalculateStat(stat);
    }
}