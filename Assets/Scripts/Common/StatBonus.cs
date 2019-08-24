using System.Collections.Generic;

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
            mult *= (1f + i / 100f);
        CurrentMultiplier = mult;
    }

    public int CalculateStat(int stat)
    {
        return (int)CalculateStat((float)stat);
    }

    public float CalculateStat(float stat)
    {
        this.isStatOutdated = false;
        if (HasFixedModifier)
        {
            return FixedModifier;
        }
        return (stat + FlatModifier) * (1f + AdditiveModifier / 100f) * CurrentMultiplier;
    }
}