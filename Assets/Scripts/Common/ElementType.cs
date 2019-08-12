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

public class ElementResistances
{
    private readonly Dictionary<ElementType, int> resists;
    private readonly Dictionary<ElementType, int> resistCaps;

    public ElementResistances()
    {
        resists = new Dictionary<ElementType, int>();
        resistCaps = new Dictionary<ElementType, int>();
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            resists[(ElementType)i] = 0;
            resistCaps[(ElementType)i] = 80;
        }
    }

    public ElementResistances(Dictionary<ElementType, int> value)
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

    public void SetResistance(ElementType e, int value)
    {
        resists[e] = value;
    }

    public int this[ElementType i]
    {
        get { return GetResistance(i); }
        set { resists[i] = value; }
    }
}