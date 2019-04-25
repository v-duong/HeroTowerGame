﻿using System;
using System.Collections.Generic;

public class StatBonus
{
    public float FlatModifier { get; private set; }
    public int AdditiveModifier { get; private set; }
    public List<float> MultiplyModifiers { get; private set; }
    public float CurrentMultiplier { get; private set; }
    public bool isStatOutdated;
    public bool hasSetModifier;
    public int setModifier;
    
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

    public void AddToFlat(double value)
    {
        FlatModifier += (float)value;
        isStatOutdated = true;
    }

    public void AddToAdditive(int value)
    {
        AdditiveModifier += value;
        isStatOutdated = true;
    }

    public void AddToMultiply(double value)
    {
        MultiplyModifiers.Add((float)value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    public void RemoveFromMultiply(int value)
    {
        MultiplyModifiers.Remove(value);
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
        return (int)Math.Round(CalculateStat((double)stat), MidpointRounding.AwayFromZero);
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