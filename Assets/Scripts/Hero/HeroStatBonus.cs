using System.Collections.Generic;

public class HeroStatBonus
{
    public int FlatModifier { get; private set; }
    public int FlatModifierFromAttributes { get; private set; }
    public int AdditiveModifier { get; private set; }
    public int AdditiveModifierFromAttributes { get; private set; }
    public List<int> MultiplyModifiers { get; private set; }
    public float CurrentMultiplier { get; private set; }
    public bool hasSetModifier;
    public int setModifier;
    public bool isStatOutdated;

    public HeroStatBonus()
    {
        FlatModifier = 0;
        FlatModifierFromAttributes = 0;
        AdditiveModifier = 0;
        AdditiveModifierFromAttributes = 0;
        MultiplyModifiers = new List<int>();
        CurrentMultiplier = 1.00f;
        hasSetModifier = false;
        setModifier = 0;
        isStatOutdated = true;
    }

    public void AddToFlat(int value)
    {
        FlatModifier += value;
        isStatOutdated = true;
    }

    public void SetFlatAttributes(int value)
    {
        FlatModifierFromAttributes = value;
        isStatOutdated = true;
    }

    public void AddToAdditive(int value)
    {
        AdditiveModifier += value;
        isStatOutdated = true;
    }

    public void SetAdditiveAttributes(int value)
    {
        AdditiveModifierFromAttributes = value;
        isStatOutdated = true;
    }

    public void AddToMultiply(int value)
    {
        MultiplyModifiers.Add(value);
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
        foreach (int i in MultiplyModifiers)
            mult *= (1d + i / 100d);
        CurrentMultiplier = (float)mult;
    }
}