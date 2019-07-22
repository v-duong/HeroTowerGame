using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AffixedItem : Item
{
    public List<Affix> prefixes;
    public List<Affix> suffixes;

    public bool RerollValues()
    {
        if (prefixes.Count == 0 && suffixes.Count == 0)
            return false;

        foreach (Affix affix in prefixes)
            affix.RerollValue();

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
            return temp[Random.Range(0, temp.Count - 1)];
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
        return RemoveAffix(GetRandomAffix());
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


        prefixes.Clear();
        suffixes.Clear();
        if (setRarityToNormal)
        {
            Rarity = RarityType.NORMAL;
            UpdateName();
        }

        UpdateItemStats();


        return true;
    }

    public bool RerollAffixesAtRarity()
    {
        int affixCap = GetAffixCap();

        if (Rarity == RarityType.NORMAL)
            return false;
        if (Rarity == RarityType.UNCOMMON)
        {
            
            ClearAffixes(false);
            AddRandomAffix();
            if (Random.Range(0, 2) == 0)
                AddRandomAffix();
            UpdateName();
            return true;
        }
        else if (Rarity == RarityType.RARE || Rarity == RarityType.EPIC)
        {
            ClearAffixes(false);
            for(int j = 0; j < affixCap+1; j++)
                AddRandomAffix();
            int i = 0;
            while ((Random.Range(0, 2) == 0) && i < (affixCap - 1))
            {
                AddRandomAffix();
                i++;
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
            returnList.Add(affix.Base.BonusTagType);
        }
        return returnList;
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
