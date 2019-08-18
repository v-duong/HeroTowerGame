using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AffixedItem : Item
{
    public List<Affix> prefixes;
    public List<Affix> suffixes;
    private bool currentPrefixesAreImmutable = false;
    private bool currentSuffixesAreImmutable = false;

    public bool RerollValues()
    {
        if (prefixes.Count == 0 && suffixes.Count == 0)
            return false;

        if (!currentPrefixesAreImmutable)
            foreach (Affix affix in prefixes)
                affix.RerollValue();

        if (!currentSuffixesAreImmutable)
            foreach (Affix affix in suffixes)
                affix.RerollValue();

        UpdateItemStats();

        return true;
    }

    public Affix GetRandomAffix()
    {
        if (prefixes.Count > 0 || suffixes.Count > 0)
        {
            List<Affix> temp = prefixes.Concat(suffixes).ToList();
            return temp[Random.Range(0, temp.Count)];
        }
        else
            return null;
    }

    public Affix GetRandomAffix(AffixType type)
    {
        if (type == AffixType.PREFIX && prefixes.Count > 0)
            return prefixes[Random.Range(0, prefixes.Count)];
        else if (type == AffixType.SUFFIX && suffixes.Count > 0)
            return suffixes[Random.Range(0, suffixes.Count)];
        else
            return null;
    }

    public bool RemoveRandomAffix()
    {
        Affix affixToRemove;

        if (currentPrefixesAreImmutable && currentSuffixesAreImmutable)
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
        if (prefixes.Count == 0 && suffixes.Count == 0 && Rarity == RarityType.NORMAL)
            return false;

        if (!currentPrefixesAreImmutable)
            prefixes.Clear();
        if (!currentSuffixesAreImmutable)
            suffixes.Clear();

        if (setRarityToNormal && prefixes.Count == 0 && suffixes.Count == 0)
        {
            Rarity = RarityType.NORMAL;
            UpdateName();
        }

        UpdateItemStats();

        return true;
    }

    public bool SetRarityToNormal()
    {
        return ClearAffixes(true);
    }

    public bool RerollAffixesAtRarity()
    {
        int affixCap = GetAffixCap();
        int affixCount = 0;

        if (Rarity == RarityType.NORMAL)
            return false;
        if (Rarity == RarityType.UNCOMMON)
        {
            ClearAffixes(false);
            affixCount = prefixes.Count + suffixes.Count;
            if (affixCount == 0)
                AddRandomAffix();
            if (affixCount == 1 && Random.Range(0, 2) == 0)
                AddRandomAffix();
            UpdateName();
            return true;
        }
        else if (Rarity == RarityType.RARE || Rarity == RarityType.EPIC)
        {
            ClearAffixes(false);
            affixCount = prefixes.Count + suffixes.Count;

            // rolls the mimimum of 4 for rare and 5 for epics
            for (int j = affixCount; j < affixCap + 1; j++)
                AddRandomAffix();

            affixCount = prefixes.Count + suffixes.Count;

            // 50% roll to continue rolling affixes
            while ((Random.Range(0, 2) == 0) && affixCount < (affixCap * 2))
            {
                AddRandomAffix();
                affixCount = prefixes.Count + suffixes.Count;
            }
            UpdateName();
            return true;
        }
        return false;
    }

    public virtual bool AddRandomAffix()
    {
        AffixType? affixType = GetRandomOpenAffixType();
        if (affixType == null)
            return false;

        return AddAffix(ResourceManager.Instance.GetRandomAffixBase((AffixType)affixType, ItemLevel, GetGroupTypes(), GetBonusTagTypeList((AffixType)affixType)));
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

    public bool UpgradeNormalToRare()
    {
        if (Rarity != RarityType.NORMAL)
            return false;
        Rarity = RarityType.RARE;
        AddRandomAffix();
        AddRandomAffix();
        AddRandomAffix();
        AddRandomAffix();
        if (Random.Range(0, 2) == 0)
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
            prefixes.Add(new Affix(affix));
        }
        else if (affix.affixType == AffixType.SUFFIX)
        {
            suffixes.Add(new Affix(affix));
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
        prefixes = prefixes.OrderByDescending(x => x.Base.affixBonuses[0].bonusType).ToList();
        suffixes = suffixes.OrderByDescending(x => x.Base.affixBonuses[0].bonusType).ToList();
    }

    public abstract bool UpgradeRarity();

    public abstract bool UpdateItemStats();

    public abstract HashSet<GroupType> GetGroupTypes();

    public abstract void UpdateName();
}