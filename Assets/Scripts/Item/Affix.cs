using System;
using System.Collections.Generic;

public class Affix
{
    public AffixBase Base;
    private readonly List<float> affixValues;
    private readonly List<float> addedEffectValues;
    public bool IsCrafted { get; private set; }
    public bool IsLocked { get; private set; }
    public AffixType AffixType { get; private set; }

    public Affix(AffixBase affixBase, bool isCrafted = false, bool isLocked = false)
    {
        Base = affixBase;
        affixValues = new List<float>();
        AffixType = affixBase.affixType;
        IsCrafted = isCrafted;
        IsLocked = isLocked;
        foreach (AffixBonusProperty mod in affixBase.affixBonuses)
        {
            if (mod.readAsFloat)
            {
                int roll = UnityEngine.Random.Range((int)(mod.minValue * 10f), (int)(mod.maxValue * 10f + 1));
                affixValues.Add((float)Math.Round(roll / 10d, 1));
            }
            else
                affixValues.Add(UnityEngine.Random.Range((int)mod.minValue, (int)mod.maxValue + 1));
        }

        if (affixBase.triggeredEffects.Count > 0)
        {
            addedEffectValues = new List<float>();
            foreach (TriggeredEffectBonusProperty addedEffect in affixBase.triggeredEffects)
            {
                int roll = UnityEngine.Random.Range((int)(addedEffect.effectMinValue * 10f), (int)(addedEffect.effectMaxValue * 10f + 1));
                addedEffectValues.Add((float)Math.Round(roll / 10d, 1));
            }
        }
    }

    public Affix(AffixBase a, List<float> values, List<float> effectValues, bool isCrafted, bool isLocked)
    {
        Base = a;
        affixValues = new List<float>();
        AffixType = a.affixType;
        IsCrafted = isCrafted;
        IsLocked = isLocked;
        int i = 0;
        foreach (AffixBonusProperty mod in a.affixBonuses)
        {
            if (mod.readAsFloat)
                affixValues.Add(values[i]);
            else
                affixValues.Add((int)values[i]);
            i++;
        }
        i = 0;
        if (a.triggeredEffects.Count > 0)
        {
            addedEffectValues = new List<float>();
            foreach (TriggeredEffectBonusProperty triggeredEffectBonus in a.triggeredEffects)
            {
                if (triggeredEffectBonus.readAsFloat)
                    addedEffectValues.Add(effectValues[i]);
                else
                    addedEffectValues.Add((int)effectValues[i]);
            }
        }
    }

    public void RerollValue()
    {
        List<AffixBonusProperty> bonuses = Base.affixBonuses;
        for (int i = 0; i < bonuses.Count; i++)
        {
            AffixBonusProperty mod = bonuses[i];
            if (mod.readAsFloat)
            {
                float roll = UnityEngine.Random.Range((int)Math.Round(mod.minValue * 10f, 0), (int)Math.Round(mod.maxValue * 10f, 0) + 1);
                affixValues[i] = (float)Math.Round(roll, 1);
            }
            else
                affixValues[i] = UnityEngine.Random.Range((int)mod.minValue, (int)mod.maxValue + 1);
        }
        List<TriggeredEffectBonusProperty> effects = Base.triggeredEffects;
        for (int i = 0; i < effects.Count; i++)
        {
            TriggeredEffectBonusProperty addedEffect = effects[i];
            int roll = UnityEngine.Random.Range((int)(addedEffect.effectMinValue * 10f), (int)(addedEffect.effectMaxValue * 10f + 1));
            addedEffectValues[i] = (float)Math.Round(roll / 10d, 1);
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

    public IList<float> GetEffectValues()
    {
        if (addedEffectValues != null)
            return addedEffectValues.AsReadOnly();
        else
            return null;
    }

    public float GetEffectValue(int index)
    {
        return addedEffectValues[index];
    }

    public void SetAffixLock(bool value)
    {
        IsLocked = value;
    }

    public static string BuildAffixString(AffixBase Base, float indent, Affix instancedAffix = null, IList<float> affixValues = null, IList<float> effectValues = null, bool showRange = false, bool showTier = false)
    {
        List<int> bonusesToSkip = new List<int>();
        string s = "";

        if (showTier)
        {
            indent += 2f;
            if (Base.tier > 0)
                s += " T" + Base.tier;
        }

        if (indent != 0)
            s += "<indent=" + indent + "em>";

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

                    if (bonusProp.restriction != GroupType.NO_GROUP)
                    {
                        s += LocalizationManager.Instance.GetLocalizationText_GroupTypeRestriction(bonusProp.restriction.ToString()) + ", ";
                    }

                    s += LocalizationManager.Instance.GetLocalizationText("bonusType." + bonusProp.bonusType.ToString().Replace("_MIN", "")) + " ";
                    if (affixValues != null)
                    {
                        s += "<nobr>+" + affixValues[i] + "~" + affixValues[matchedIndex] + "</nobr>";
                        if (showRange && (bonusProp.minValue != bonusProp.maxValue || Base.affixBonuses[matchedIndex].minValue != Base.affixBonuses[matchedIndex].maxValue))
                            s += "<nobr><color=" + Helpers.AFFIX_RANGE_COLOR + "> (" + bonusProp.minValue + "-" + bonusProp.maxValue + ") (" + Base.affixBonuses[matchedIndex].minValue + "-" + Base.affixBonuses[matchedIndex].maxValue + ")</color></nobr>";
                    }
                    else
                        s += "<nobr>+(" + bonusProp.minValue + "-" + bonusProp.maxValue + ")~(" + Base.affixBonuses[matchedIndex].minValue + "-" + Base.affixBonuses[matchedIndex].maxValue + ")</nobr>";

                    s += '\n';
                    continue;
                }
            }

            if (affixValues != null)
            {
                s += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProp.bonusType, bonusProp.modifyType, affixValues[i], bonusProp.restriction);
                if (showRange && bonusProp.minValue != bonusProp.maxValue)
                    s = s.TrimEnd('\n') + "<nobr><color=" + Helpers.AFFIX_RANGE_COLOR + "> (" + bonusProp.minValue + "-" + bonusProp.maxValue + ")</color></nobr>\n";
            }
            else
                s += LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProp.bonusType, bonusProp.modifyType, bonusProp.minValue, bonusProp.maxValue, bonusProp.restriction);
        }

        for (int i = 0; i < Base.triggeredEffects.Count; i++)
        {
            TriggeredEffectBonusProperty triggeredEffect = Base.triggeredEffects[i];

            if (effectValues != null)
                s += LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(triggeredEffect, effectValues[i]);
            else
                s += LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(triggeredEffect, triggeredEffect.effectMinValue, triggeredEffect.effectMaxValue);

            if (showRange && triggeredEffect.effectMinValue != triggeredEffect.effectMaxValue)
                s = s.TrimEnd('\n') + "<nobr><color=" + Helpers.AFFIX_RANGE_COLOR + "> (" + triggeredEffect.effectMinValue + "-" + triggeredEffect.effectMaxValue + ")</color></nobr>\n";
        }

        if (instancedAffix != null)
        {
            if (instancedAffix.IsLocked)
                s = "<color=#A9942C>" + s + "</color>";
        }

        if (indent != 0)
            s = s.TrimEnd('\n') + "</indent>\n";

        return s;
    }
}