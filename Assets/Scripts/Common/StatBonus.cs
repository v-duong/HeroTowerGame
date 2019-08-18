using System;
using System.Collections.Generic;

public class StatBonus
{
    public float FlatModifier { get; private set; }
    public int AdditiveModifier { get; private set; }
    public List<float> MultiplyModifiers { get; private set; }
    public float CurrentMultiplier { get; private set; }
    public bool isStatOutdated;
    public bool hasSetModifier;
    public float setModifier;

    public StatBonus()
    {
        FlatModifier = 0;
        AdditiveModifier = 0;
        MultiplyModifiers = new List<float>();
        CurrentMultiplier = 1.00f;
        hasSetModifier = false;
        setModifier = 0;
        isStatOutdated = true;
    }

    public void ResetBonus()
    {
        FlatModifier = 0;
        AdditiveModifier = 0;
        MultiplyModifiers.Clear();
        CurrentMultiplier = 1.00f;
        hasSetModifier = false;
        setModifier = 0;
        isStatOutdated = true;
    }

    public void AddBonus(ModifyType type, double value)
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

            case ModifyType.SET:
                AddSetBonus(value);
                return;
        }
    }


    public void RemoveBonus(ModifyType type, double value)
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

            case ModifyType.SET:
                RemoveSetBonus(value);
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

    private void AddSetBonus(double value)
    {
        hasSetModifier = true;
        setModifier = (float)value;
    }

    private void RemoveSetBonus(double value)
    {
        hasSetModifier = false;
        setModifier = (float)value;
    }

    private void AddToFlat(double value)
    {
        FlatModifier += (float)value;
        isStatOutdated = true;
    }

    private void AddToAdditive(int value)
    {
        AdditiveModifier += value;
        isStatOutdated = true;
    }

    private void AddToMultiply(double value)
    {
        MultiplyModifiers.Add((float)value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    private void RemoveFromMultiply(double value)
    {
        MultiplyModifiers.Remove((float)value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    public void UpdateCurrentMultiply()
    {
        double mult = 1.0d;
        foreach (double i in MultiplyModifiers)
            mult *= (1d + i / 100d);
        CurrentMultiplier = (float)mult;
    }

    public void SetMultiply(double value)
    {
        CurrentMultiplier = (float)value;
    }

    public int CalculateStat(int stat)
    {
        return (int)CalculateStat((double)stat);
    }

    public double CalculateStat(double stat)
    {
        if (this.hasSetModifier)
        {
            this.isStatOutdated = false;
            return this.setModifier;
        }
        this.isStatOutdated = false;
        return (stat + this.FlatModifier) * (1 + (double)(this.AdditiveModifier) / 100) * this.CurrentMultiplier;
    }
}