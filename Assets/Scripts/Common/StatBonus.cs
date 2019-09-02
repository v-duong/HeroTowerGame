using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatBonus
{
    public float FlatModifier { get; private set; }
    public int AdditiveModifier { get; private set; }
    public List<float> MultiplyModifiers { get; private set; }
    public float CurrentMultiplier { get; private set; }
    public bool isStatOutdated;
    public bool HasFixedModifier { get; private set; }
    public float FixedModifier { get; private set; }

    public StatBonus()
    {
        FlatModifier = 0;
        AdditiveModifier = 0;
        MultiplyModifiers = new List<float>();
        CurrentMultiplier = 1.00f;
        HasFixedModifier = false;
        FixedModifier = 0;
        isStatOutdated = true;
    }

    public void ResetBonus()
    {
        FlatModifier = 0;
        AdditiveModifier = 0;
        MultiplyModifiers.Clear();
        CurrentMultiplier = 1.00f;
        HasFixedModifier = false;
        FixedModifier = 0;
        isStatOutdated = true;
    }

    public void AddBonuses(StatBonus otherBonus, bool overwriteFixed = true)
    {
        FlatModifier += otherBonus.FlatModifier;
        AdditiveModifier += otherBonus.AdditiveModifier;
        MultiplyModifiers.AddRange(otherBonus.MultiplyModifiers);
        if (otherBonus.HasFixedModifier && (HasFixedModifier && overwriteFixed || !HasFixedModifier))
        {
            HasFixedModifier = true;
            FixedModifier = otherBonus.FixedModifier;
        }
    }

    public void AddBonus(ModifyType type, float value)
    {
        switch (type)
        {
            case ModifyType.FLAT_ADDITION:
                AddToFlat(value);
                return;

            case ModifyType.ADDITIVE:
                AddToAdditive((int)value);
                return;

            case ModifyType.MULTIPLY:
                AddToMultiply(value);
                return;

            case ModifyType.FIXED_TO:
                AddFixedBonus(value);
                return;
        }
    }

    public void RemoveBonus(ModifyType type, float value)
    {
        switch (type)
        {
            case ModifyType.FLAT_ADDITION:
                AddToFlat(-value);
                return;

            case ModifyType.ADDITIVE:
                AddToAdditive((int)-value);
                return;

            case ModifyType.MULTIPLY:
                RemoveFromMultiply(value);
                return;

            case ModifyType.FIXED_TO:
                RemoveFixedBonus(value);
                return;
        }
    }

    public void SetFlat(int value)
    {
        FlatModifier = value;
        isStatOutdated = true;
    }

    public void SetAdditive(int value)
    {
        AdditiveModifier = value;
        isStatOutdated = true;
    }

    private void AddFixedBonus(float value)
    {
        HasFixedModifier = true;
        FixedModifier = value;
    }

    private void RemoveFixedBonus(float value)
    {
        HasFixedModifier = false;
        FixedModifier = value;
    }

    private void AddToFlat(float value)
    {
        FlatModifier += value;
        isStatOutdated = true;
    }

    private void AddToAdditive(int value)
    {
        AdditiveModifier += value;
        isStatOutdated = true;
    }

    private void AddToMultiply(float value)
    {
        MultiplyModifiers.Add(value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    private void RemoveFromMultiply(float value)
    {
        MultiplyModifiers.Remove(value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    public void UpdateCurrentMultiply()
    {
        float mult = 1.0f;
        foreach (float i in MultiplyModifiers)
            mult *= 1f + i / 100f;
        CurrentMultiplier = mult;
    }

    public int CalculateStat(int stat)
    {
        return (int)CalculateStat((float)stat);
    }

    public float CalculateStat(float stat)
    {
        isStatOutdated = false;
        if (HasFixedModifier)
        {
            return FixedModifier;
        }
        return (stat + FlatModifier) * (1f + AdditiveModifier / 100f) * CurrentMultiplier;
    }
}

public class StatBonusCollection
{
    private Dictionary<GroupType, StatBonus> statBonuses;

    public StatBonusCollection()
    {
        statBonuses = new Dictionary<GroupType, StatBonus>();
    }

    public IEnumerable<GroupType> GetGroupTypeIntersect(IEnumerable<GroupType> types)
    {
        return statBonuses.Keys.Intersect(types);
    }

    public void AddBonus(GroupType type, ModifyType modifyType, float value)
    {
        if (!statBonuses.ContainsKey(type))
            statBonuses[type] = new StatBonus();
        statBonuses[type].AddBonus(modifyType, value);
    }

    public bool RemoveBonus(GroupType type, ModifyType modifyType, float value)
    {
        if (statBonuses.ContainsKey(type))
        {
            statBonuses[type].RemoveBonus(modifyType, value);
            return true;
        }
        else
            return false;
    }

    public StatBonus GetStatBonus(GroupType type)
    {
        if (statBonuses.ContainsKey(type))
            return statBonuses[type];
        else
            return null;
    }

    public StatBonus GetTotalStatBonus(IEnumerable<GroupType> groupTypes)
    {
        StatBonus returnBonus = new StatBonus();

        if (groupTypes == null)
            return returnBonus;

        var intersectingTypes = GetGroupTypeIntersect(groupTypes).ToList();
        if (intersectingTypes.Count == 0)
            return returnBonus;

        foreach (GroupType type in intersectingTypes)
        {
            returnBonus.AddBonuses(statBonuses[type]);
        }
        returnBonus.UpdateCurrentMultiply();

        return returnBonus;
    }
}