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
    public GroupType ItemType { get; protected set; }
    
    public List<Affix> prefixes;
    public List<Affix> suffixes;

    public virtual void SetRarity(RarityType rarity) => Rarity = rarity;

    public void RerollValues()
    {
        foreach (Affix affix in prefixes)
            affix.RerollValue();

        foreach (Affix affix in suffixes)
            affix.RerollValue();
    }

    public Affix GetRandomAffix()
    {
        if (prefixes.Count > 0 && suffixes.Count > 0)
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
            return prefixes[Random.Range(0, prefixes.Count - 1)];
        else if (type == AffixType.SUFFIX && suffixes.Count > 0)
            return suffixes[Random.Range(0, suffixes.Count - 1)];
        else
            return null;
    }

    public void RemoveAffix(Affix affix)
    {
        if (affix.AffixType == AffixType.PREFIX && prefixes.Contains(affix))
        {
            prefixes.Remove(affix);
        }
        else if (affix.AffixType == AffixType.SUFFIX && suffixes.Contains(affix))
        {
            suffixes.Remove(affix);
        }
        else
            return;
    }

    public void ClearAffixes(bool setRarityToNormal = true)
    {
        prefixes.Clear();
        suffixes.Clear();
        if (setRarityToNormal)
            Rarity = RarityType.NORMAL;
    }

    public virtual void InitializeAtRarity(RarityType rarity)
    {
        
    }

    public virtual void AddRandomAffix()
    {
        int i = Random.Range(0, 2);
    }

    public virtual void AddAffix(AffixBase affix)
    {
        if (affix.affixType == AffixType.PREFIX)
        {
            prefixes.Add(new Affix(affix));
        }
        else if (affix.affixType == AffixType.SUFFIX)
        {
            suffixes.Add(new Affix(affix));
        }
        else
            return;
    }

    

}

public enum RarityType
{
    NORMAL,
    UNCOMMON,
    RARE,
    EPIC
}