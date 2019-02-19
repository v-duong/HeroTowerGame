﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Affix
{
    public AffixBase Base { get { return ResourceManager.Instance.GetAffixBase(BaseId, AffixType); } }
    public int BaseId { get; private set; }
    private Dictionary<BonusType, int> affixValues;
    public AffixType AffixType { get; private set; }


    public Affix(AffixBase a)
    {
        BaseId = a.id;
        affixValues = new Dictionary<BonusType, int>();
        AffixType = a.affixType;
        foreach (AffixBonusBase mod in a.affixBonuses)
        {
            affixValues.Add(mod.bonusType, Random.Range(mod.minValue, mod.maxValue + 1));
        }
    }

    public void RerollValue()
    {
        List<AffixBonusBase> temp = Base.affixBonuses;
        foreach (AffixBonusBase mod in temp)
        {
            affixValues[mod.bonusType] = Random.Range(mod.minValue, mod.maxValue + 1);
        }
    }

    public Dictionary<BonusType, int> GetAffixValues()
    {
        return new Dictionary<BonusType, int>(affixValues);
    }

    public int GetAffixValue(BonusType type)
    {
        return affixValues[type];
    }
}

