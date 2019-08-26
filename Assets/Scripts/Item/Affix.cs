using System.Collections.Generic;
using UnityEngine;

public class Affix
{
    public AffixBase Base;
    private Dictionary<BonusType, float> affixValues;
    public AffixType AffixType { get; private set; }

    public Affix(AffixBase a, bool locked = false)
    {
        Base = a;
        affixValues = new Dictionary<BonusType, float>();
        AffixType = a.affixType;
        foreach (AffixBonusProperty mod in a.affixBonuses)
        {
            if (mod.readAsFloat)
                affixValues.Add(mod.bonusType, Random.Range(mod.minValue, mod.maxValue ));
            else
                affixValues.Add(mod.bonusType, (int)Random.Range(mod.minValue, mod.maxValue));
        }
    }

    public void RerollValue()
    {
        List<AffixBonusProperty> temp = Base.affixBonuses;
        foreach (AffixBonusProperty mod in temp)
        {
            affixValues[mod.bonusType] = Random.Range(mod.minValue, mod.maxValue + 1);
        }
    }

    public Dictionary<BonusType, float> GetAffixValues()
    {
        return new Dictionary<BonusType, float>(affixValues);
    }

    public float GetAffixValue(BonusType type)
    {
        return affixValues[type];
    }

    public string BuildAffixString(bool useTab = true)
    {
        string s = "○ ";
        foreach (AffixBonusProperty b in Base.affixBonuses)
        {
            if (b.bonusType.ToString().Contains("DAMAGE_MAX"))
            {
                continue;
            }
            if (b.bonusType.ToString().Contains("DAMAGE_MIN"))
            {
                if (useTab)
                    s += "\t";
                s += LocalizationManager.Instance.GetLocalizationText("bonusType." + b.bonusType) + " ";
                s += "+" + GetAffixValue(b.bonusType) + "-" + GetAffixValue(b.bonusType + 1) + "\n";
            }
            else
            {
                if (useTab)
                    s += "\t";
                s += LocalizationManager.Instance.GetLocalizationText_BonusType(b.bonusType, b.modifyType, GetAffixValue(b.bonusType));
            }
        }
        return s;
    }

}