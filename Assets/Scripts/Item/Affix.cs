using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Affix
{
    public AffixBase Base { get { return ResourceManager.Instance.GetAffixBase(BaseId, AffixType); } }
    public int BaseId { get; private set; }
    private List<int> affixValue;
    public AffixType AffixType { get; private set; }


    public Affix(AffixBase a)
    {
        BaseId = a.id;
        affixValue = new List<int>();
        AffixType = a.affixType;
        foreach (AffixBonus mod in a.affixBonuses)
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

    public List<int> GetAffixValues()
    {
        return new List<int>(affixValue);
    }

}


