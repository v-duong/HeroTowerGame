using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Affix
{
    public AffixBase Base;
    public List<int> affixValue;
    public AffixType AffixType => Base.affixType;


    Affix(AffixBase a)
    {
        Base = a;
        affixValue = new List<int>();
        foreach (AffixBonus mod in Base.affixBonuses)
        {
            affixValue.Add(Random.Range(mod.minValue, mod.maxValue + 1));
        }
    }

    public void RerollValue()
    {
        List<AffixBonus> temp = Base.affixBonuses;
        for (int i = 0; i < temp.Count; i++)
        {
            affixValue[i] = Random.Range(temp[i].minValue, temp[i].maxValue + 1);
        }
    }

    
}


