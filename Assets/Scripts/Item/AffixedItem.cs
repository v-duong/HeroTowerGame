using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AffixedItem : Item
{
    public List<Affix> prefixes;
    public List<Affix> suffixes;
    private bool currentPrefixesAreImmutable = false;
    private bool currentSuffixesAreImmutable = false;

    public abstract bool UpgradeRarity();

    public abstract bool UpdateItemStats();

    public abstract HashSet<GroupType> GetGroupTypes();

    public abstract void UpdateName();

    public virtual List<Affix> GetAllAffixes()
    {
        List<Affix> affixes = new List<Affix>();
        affixes.AddRange(prefixes);
        affixes.AddRange(suffixes);
        return affixes;
    }

    public bool RerollValues()
    {
        if (prefixes.Count == 0 && suffixes.Count == 0)
            return false;

        if (!currentPrefixesAreImmutable)
            foreach (Affix affix in prefixes.Where(x => !x.IsLocked))
                affix.RerollValue();

        if (!currentSuffixesAreImmutable)
            foreach (Affix affix in suffixes.Where(x => !x.IsLocked))
                affix.RerollValue();

        UpdateItemStats();

        return true;
    }

    public Affix GetRandomAffix()
    {
        if (Rarity == RarityType.UNIQUE)
            return null;

        List<Affix> temp = prefixes.Concat(suffixes).Where(x => !x.IsLocked).ToList();
        if (temp.Count > 0)
        {
            return temp[Random.Range(0, temp.Count)];
        }
        else
            return null;
    }

    public Affix GetRandomAffix(AffixType type)
    {
        List<Affix> listToRemoveFrom;
        if (type == AffixType.PREFIX && prefixes.Count > 0)
        {
            listToRemoveFrom = prefixes;
        }
        else if (type == AffixType.SUFFIX && suffixes.Count > 0)
        {
            listToRemoveFrom = suffixes;
        }
        else
        {
            return null;
        }

        var newList = listToRemoveFrom.Where(x => !x.IsLocked).ToList();

        if (newList.Count == 0)
            return null;

        return newList[Random.Range(0, newList.Count)];
    }

    public bool RemoveRandomAffix()
    {
        Affix affixToRemove;

        if (currentPrefixesAreImmutable && currentSuffixesAreImmutable || Rarity == RarityType.UNIQUE)
            return false;
        else if (currentPrefixesAreImmutable)
            affixToRemove = GetRandomAffix(AffixType.SUFFIX);
        else if (currentSuffixesAreImmutable)
            affixToRemove = GetRandomAffix(AffixType.PREFIX);
        else
            affixToRemove = GetRandomAffix();

        return RemoveAffix(affixToRemove);
    }

    public bool RemoveAffix(Affix affix)
    {
        if (affix == null)
            return false;
        if (affix.AffixType == AffixType.PREFIX && prefixes.Contains(affix))
        {
            prefixes.Remove(affix);
        }
        else if (affix.AffixType == AffixType.SUFFIX && suffixes.Contains(affix))
        {
            suffixes.Remove(affix);
        }
        else
            return false;
        SortAffixes();
        UpdateItemStats();
        return true;
    }

    public bool ClearAffixes(bool setRarityToNormal = true)
    {
        if (prefixes.Count == 0 && suffixes.Count == 0 && Rarity == RarityType.NORMAL || Rarity == RarityType.UNIQUE)
            return false;

        if (!currentPrefixesAreImmutable)
            prefixes.RemoveAll(x => !x.IsLocked);
        if (!currentSuffixesAreImmutable)
            suffixes.RemoveAll(x => !x.IsLocked);

        if (setRarityToNormal && prefixes.Count == 0 && suffixes.Count == 0)
        {
            Rarity = RarityType.NORMAL;
            UpdateName();
        }
        else if (setRarityToNormal && (prefixes.Count + suffixes.Count) <= 2)
        {
            Rarity = RarityType.UNCOMMON;
            UpdateName();
        }

        UpdateItemStats();

        return true;
    }

    public bool SetRarityToNormal()
    {
        return ClearAffixes(true);
    }

    public bool RerollAffixesAtRarity(Dictionary<GroupType, float> weightModifiers = null, float affixLevelSkewFactor = 1f, HashSet<GroupType> additionalTypes = null)
    {
        int affixCap = GetAffixCap();

        if (Rarity == RarityType.NORMAL || Rarity == RarityType.UNIQUE)
            return false;

        int affixCount;

        if (Rarity == RarityType.UNCOMMON)
        {
            ClearAffixes(false);
            affixCount = prefixes.Count + suffixes.Count;
            if (affixCount == 0)
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);
            if (affixCount == 1 && Random.Range(0, 2) == 0)
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);
            UpdateName();
            return true;
        }
        else if (Rarity == RarityType.RARE || Rarity == RarityType.EPIC)
        {
            ClearAffixes(false);
            affixCount = prefixes.Count + suffixes.Count;

            // rolls the mimimum of 4 for rare and 5 for epics
            for (int j = affixCount; j < affixCap + 1; j++)
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);

            affixCount = prefixes.Count + suffixes.Count;

            // 50% roll to continue rolling affixes
            while ((Random.Range(0, 2) == 0) && affixCount < (affixCap * 2))
            {
                AddRandomAffix(weightModifiers, affixLevelSkewFactor, additionalTypes);
                affixCount = prefixes.Count + suffixes.Count;
            }
            UpdateName();
            return true;
        }
        return false;
    }

    public virtual bool AddRandomAffix(Dictionary<GroupType, float> weightModifiers = null, float affixLevelSkewFactor = 1f, HashSet<GroupType> additionalGroupTypes = null)
    {
        AffixType? affixType = GetRandomOpenAffixType();
        if (affixType == null)
            return false;

        HashSet<GroupType> groupTypes = GetGroupTypes();
        if (additionalGroupTypes != null)
            groupTypes.UnionWith(additionalGroupTypes);

        return AddAffix(ResourceManager.Instance.GetRandomAffixBase((AffixType)affixType, ItemLevel, groupTypes, GetBonusTagTypeList((AffixType)affixType)));
    }

    public List<string> GetBonusTagTypeList(AffixType type)
    {
        List<Affix> list;
        List<string> returnList = new List<string>();
        if (type == AffixType.PREFIX)
            list = prefixes;
        else
            list = suffixes;
        foreach (Affix affix in list)
        {
            returnList.Add(affix.Base.AffixBonusTypeString);
        }
        return returnList;
    }

    public bool UpgradeNormalToUncommon()
    {
        if (Rarity != RarityType.NORMAL)
            return false;
        Rarity = RarityType.UNCOMMON;
        AddRandomAffix();
        if (Random.Range(0, 2) == 0)
            AddRandomAffix();
        UpdateName();
        return true;
    }

    public virtual int GetAffixCap()
    {
        switch (Rarity)
        {
            case RarityType.EPIC:
                return 4;

            case RarityType.RARE:
                return 3;

            case RarityType.UNCOMMON:
                return 1;

            case RarityType.NORMAL:
            default:
                return 0;
        }
    }

    public AffixType? GetRandomOpenAffixType()
    {
        if (Rarity == RarityType.UNIQUE)
            return null;

        int affixCap = GetAffixCap();

        if (prefixes.Count < affixCap && suffixes.Count < affixCap)
        {
            int i = Random.Range(0, 2);
            if (i == 0)
                return AffixType.PREFIX;
            else
                return AffixType.SUFFIX;
        }
        else if (prefixes.Count >= affixCap && suffixes.Count < affixCap)
        {
            return AffixType.SUFFIX;
        }
        else if (suffixes.Count >= affixCap && prefixes.Count < affixCap)
        {
            return AffixType.PREFIX;
        }
        else
            return null;
    }

    public bool AddAffix(AffixBase affix)
    {
        if (affix == null)
            return false;

        if (affix.affixType == AffixType.PREFIX)
        {
            prefixes.Add(new Affix(affix, false));
        }
        else if (affix.affixType == AffixType.SUFFIX)
        {
            suffixes.Add(new Affix(affix, false));
        }
        else
            return false;
        SortAffixes();
        UpdateItemStats();
        return true;
    }

    private void SortAffixes()
    {
        if (Rarity == RarityType.UNIQUE)
            return;
        prefixes = prefixes.OrderBy(x => x.Base.affixBonuses.Count > 0 ? (int)x.Base.affixBonuses[0].bonusType : int.MaxValue).ToList();
        suffixes = suffixes.OrderBy(x => x.Base.affixBonuses.Count > 0 ? (int)x.Base.affixBonuses[0].bonusType : int.MaxValue).ToList();
    }

    public WeightList<AffixBase> GetAllPossiblePrefixes(Dictionary<GroupType, float> weightModifiers)
    {
        return ResourceManager.Instance.GetPossibleAffixes(AffixType.PREFIX, ItemLevel, GetGroupTypes(), GetBonusTagTypeList(AffixType.PREFIX), weightModifiers, 1f);
    }

    public WeightList<AffixBase> GetAllPossibleSuffixes(Dictionary<GroupType, float> weightModifiers)
    {
        return ResourceManager.Instance.GetPossibleAffixes(AffixType.SUFFIX, ItemLevel, GetGroupTypes(), GetBonusTagTypeList(AffixType.SUFFIX), weightModifiers, 1f);
    }

    public void RemoveAllAffixLocks()
    {
        foreach (Affix affix in GetAllAffixes())
            affix.SetAffixLock(false);
    }

    public int GetLockCount()
    {
        int lockCount = 0;
        foreach (Affix affix in GetAllAffixes())
            if (affix.IsLocked)
                lockCount++;
        return lockCount;
    }

    public static int GetToNormalCost(AffixedItem currentItem)
    {
        return currentItem.ItemLevel * 3;
    }

    public static int GetRerollAffixCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.UNCOMMON:
                return currentItem.ItemLevel * 4;

            case RarityType.RARE:
                return currentItem.ItemLevel * 15;

            case RarityType.EPIC:
                return currentItem.ItemLevel * 75;

            default:
                return 0;
        }
    }

    public static int GetAddAffixCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.UNCOMMON:
                return currentItem.ItemLevel * 3;

            case RarityType.RARE:
                return currentItem.ItemLevel * 25;

            case RarityType.EPIC:
                return currentItem.ItemLevel * 150;

            default:
                return 0;
        }
    }

    public static int GetRemoveAffixCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.UNCOMMON:
                return currentItem.ItemLevel * 3;

            case RarityType.RARE:
                return currentItem.ItemLevel * 15;

            case RarityType.EPIC:
                return currentItem.ItemLevel * 75;

            default:
                return 0;
        }
    }

    public static int GetRerollValuesCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.UNCOMMON:
                return currentItem.ItemLevel * 1;

            case RarityType.RARE:
                return currentItem.ItemLevel * 2;

            case RarityType.EPIC:
                return currentItem.ItemLevel * 3;

            case RarityType.UNIQUE:
                return currentItem.ItemLevel * 2;

            default:
                return 0;
        }
    }

    public static int GetUpgradeCost(AffixedItem currentItem)
    {
        switch (currentItem.Rarity)
        {
            case RarityType.NORMAL:
                return currentItem.ItemLevel * 1;

            case RarityType.UNCOMMON:
                return currentItem.ItemLevel * 8;

            case RarityType.RARE:
                return currentItem.ItemLevel * 500;

            default:
                return 0;
        }
    }

    public static int GetLockCost(AffixedItem currentItem)
    {
        if (currentItem.GetLockCount() >= 1)
            return 0;

        switch (currentItem.Rarity)
        {
            case RarityType.UNCOMMON:
                return currentItem.ItemLevel * 15;

            case RarityType.RARE:
                return currentItem.ItemLevel * 50;

            case RarityType.EPIC:
                return currentItem.ItemLevel * 300;

            case RarityType.UNIQUE:
                return currentItem.ItemLevel * 20;

            default:
                return 0;
        }
    }
}