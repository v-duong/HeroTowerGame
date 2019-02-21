using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Item
{
    public int id;
    public string Name { get; protected set; }
    public RarityType Rarity { get; protected set; }
    public int ItemLevel { get; protected set; }
    public GroupType Group { get; protected set; }
    
    public List<Affix> prefixes;
    public List<Affix> suffixes;

    public void SetRarity(RarityType rarity) => Rarity = rarity;

    public bool RerollValues()
    {
        if (prefixes.Count == 0 && suffixes.Count == 0)
            return false;

        foreach (Affix affix in prefixes)
            affix.RerollValue();

        foreach (Affix affix in suffixes)
            affix.RerollValue();

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
            return true;
        }
        else if (affix.AffixType == AffixType.SUFFIX && suffixes.Contains(affix))
        {
            suffixes.Remove(affix);
            return true;
        }
        else
            return false;
    }

    public bool ClearAffixes(bool setRarityToNormal = true)
    {
        if (prefixes.Count == 0 && suffixes.Count == 0 && Rarity == RarityType.NORMAL)
            return false;

        prefixes.Clear();
        suffixes.Clear();
        if (setRarityToNormal)
            Rarity = RarityType.NORMAL;

        return true;
    }

    public bool RerollAffixesAtRarity()
    {
        if (Rarity == RarityType.NORMAL)
            return false;
        if (Rarity == RarityType.UNCOMMON)
        {
            ClearAffixes(false);
            AddRandomAffix();
            if (Random.Range(0, 2) == 0)
                AddRandomAffix();
            return true;
        } else if (Rarity == RarityType.RARE)
        {
            ClearAffixes(false);
            AddRandomAffix();
            AddRandomAffix();
            AddRandomAffix();
            AddRandomAffix();
            int i = 0;
            while ((Random.Range(0, 2) == 0) && i < 2)
            {
                AddRandomAffix();
                i++;
            }
            return true;
        } else if (Rarity == RarityType.EPIC)
        {
            ClearAffixes(false);
            AddRandomAffix();
            AddRandomAffix();
            AddRandomAffix();
            AddRandomAffix();
            AddRandomAffix();
            int i = 0;
            while ((Random.Range(0, 2) == 0) && i < 3)
            {
                AddRandomAffix();
                i++;
            }
            return true;
        }
        return false;

    }

    public virtual bool AddRandomAffix()
    {
        AffixType? affixType = GetRandomOpenAffixType();
        if (affixType == null)
            return false;

        return AddAffix(ResourceManager.Instance.GetRandomAffixBase((AffixType)affixType, ItemLevel, Group, GetBonusTagTypeList((AffixType)affixType)));

    }

    public List<string> GetBonusTagTypeList(AffixType type)
    {
        List<Affix> list;
        List<string> returnList = new List<string>();
        if (type == AffixType.PREFIX)
            list = prefixes;
        else
            list = suffixes;
        foreach(Affix affix in list)
        {
            returnList.Add(affix.Base.BonusTagType);
        }
        return returnList;
    }

    public AffixType? GetRandomOpenAffixType()
    {
        int affixCap = 0;
        switch (Rarity)
        {
            case RarityType.EPIC:
                affixCap = 4;
                break;
            case RarityType.RARE:
                affixCap = 3;
                break;
            case RarityType.UNCOMMON:
                affixCap = 1;
                break;
            case RarityType.NORMAL:
            default:
                return null;
        }
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
            return true;
        }
        else if (affix.affixType == AffixType.SUFFIX)
        {
            suffixes.Add(new Affix(affix));
            return true;
        }
        else
            return false;
    }

    public virtual bool UpgradeRarity()
    {
        if (Rarity == RarityType.EPIC)
            return false;
        else
            Rarity++;
        AddRandomAffix();
        return true;
    }

}

public enum RarityType
{
    NORMAL,
    UNCOMMON,
    RARE,
    EPIC
}