using System;
using System.Collections.Generic;

public enum ElementType
{
    PHYSICAL, //none/physical
    FIRE,
    COLD,
    LIGHTNING,
    EARTH,
    DIVINE,
    VOID
}

public class ElementalData
{
    public const int DEFAULT_RESISTANCE_CAP = 80;
    public const int HARD_RESISTANCE_CAP = 95;
    private readonly Dictionary<ElementType, int> resists;
    private readonly Dictionary<ElementType, int> resistCaps;
    public readonly Dictionary<ElementType, int> negations;

    public ElementalData()
    {
        resists = new Dictionary<ElementType, int>();
        resistCaps = new Dictionary<ElementType, int>();
        negations = new Dictionary<ElementType, int>();
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            resists[element] = 0;
            resistCaps[element] = DEFAULT_RESISTANCE_CAP;
            negations[element] = 0;
        }
    }

    public ElementalData(Dictionary<ElementType, int> value)
    {
        resists = new Dictionary<ElementType, int>();
        foreach (KeyValuePair<ElementType, int> e in value)
        {
            resists[e.Key] = e.Value;
        }
    }

    public int GetResistance(ElementType e)
    {
        return Math.Min(resists[e], resistCaps[e]);
    }

    public int GetUncapResistance(ElementType e)
    {
        return resists[e];
    }

    public void SetResistanceCap(ElementType e, int value)
    {
        resistCaps[e] = Math.Min(value, HARD_RESISTANCE_CAP);
    }

    public int GetNegation(ElementType e)
    {
        return negations[e];
    }

    public void SetNegation(ElementType e, int value)
    {
        negations[e] = value;
    }

    public int this[ElementType i]
    {
        get { return GetResistance(i); }
        set { resists[i] = value; }
    }
}