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
    VOID,
    COUNT
}

public class ElementalData
{
    private readonly Dictionary<ElementType, int> resists;
    private readonly Dictionary<ElementType, int> resistCaps;
    private readonly Dictionary<ElementType, int> negations;

    public ElementalData()
    {
        resists = new Dictionary<ElementType, int>();
        resistCaps = new Dictionary<ElementType, int>();
        negations = new Dictionary<ElementType, int>();
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            resists[(ElementType)i] = 0;
            resistCaps[(ElementType)i] = 80;
            negations[(ElementType)i] = 0;
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
        resistCaps[e] = value;
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