using System;
using System.Collections.Generic;
using UnityEngine;

public class Affix
{
    public AffixBase Base;
    private List<float> affixValues;
    public AffixType AffixType { get; private set; }

    public Affix(AffixBase a, bool locked = false)
    {
        Base = a;
        affixValues = new List<float>();
        AffixType = a.affixType;
        foreach (AffixBonusProperty mod in a.affixBonuses)
        {
            if (mod.readAsFloat)
                affixValues.Add(UnityEngine.Random.Range(mod.minValue, mod.maxValue));
            else
                affixValues.Add((int)UnityEngine.Random.Range(mod.minValue, mod.maxValue));
        }
    }

    public Affix(string affixIdName, AffixType affixType, List<float> values, bool locked = false)
    {
        AffixBase a = ResourceManager.Instance.GetAffixBase(affixIdName, affixType);
        Base = a;
        affixValues = new List<float>();
        AffixType = a.affixType;
        int i = 0;
        foreach (AffixBonusProperty mod in a.affixBonuses)
        {
            if (mod.readAsFloat)
                affixValues.Add(values[i]);
            else
                affixValues.Add((int)values[i]);
            i++;
        }
    }

    public void RerollValue()
    {
        List<AffixBonusProperty> temp = Base.affixBonuses;
        for (int i = 0; i < temp.Count; i++)
        {
            AffixBonusProperty mod = temp[i];
            affixValues[i] = UnityEngine.Random.Range(mod.minValue, mod.maxValue + 1);
        }
    }

    public IList<float> GetAffixValues()
    {
        return affixValues.AsReadOnly();
    }

    public float GetAffixValue(int index)
    {
        return affixValues[index];
    }

    public string BuildAffixString(bool useTab = true)
    {
        List<int> bonusesToSkip = new List<int>();
        string s = "○ ";
        for (int i = 0; i < Base.affixBonuses.Count; i++)
        {
            if (bonusesToSkip.Contains(i))
            {
                continue;
            }
            AffixBonusProperty bonusProp = Base.affixBonuses[i];
            if (bonusProp.bonusType.ToString().Contains("DAMAGE_MIN") && bonusProp.modifyType == ModifyType.FLAT_ADDITION)
            {
                BonusType maxType = (BonusType)Enum.Parse(typeof(BonusType), bonusProp.bonusType.ToString().Replace("_MIN", "_MAX"));
                int matchedIndex = Base.affixBonuses.FindIndex(x => x.bonusType == maxType);

                if (matchedIndex > 0 && Base.affixBonuses[matchedIndex].modifyType == ModifyType.FLAT_ADDITION)
                {
                    bonusesToSkip.Add(matchedIndex);

                    if (useTab)
                        s += "\t";

                    if (bonusProp.restriction != GroupType.NO_GROUP)
                    {
                        s = LocalizationManager.Instance.GetLocalizationText_GroupTypeRestriction(bonusProp.restriction.ToString()) + ", ";
                    }

                    s += LocalizationManager.Instance.GetLocalizationText("bonusType." + bonusProp.bonusType.ToString().Replace("_MIN","")) + " ";
                    s += "+" + GetAffixValue(i) + "-" + GetAffixValue(matchedIndex) + "\n";
                    continue;
                }
            }

            if (useTab)
                s += "\t";
            s += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProp.bonusType, bonusProp.modifyType, GetAffixValue(i), bonusProp.restriction);
        }
        return s;
    }
}