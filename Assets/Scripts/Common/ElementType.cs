using System.Collections;
using System.Collections.Generic;
using System;

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
    private readonly Dictionary<ElementType, int> dict;

    public ElementResistances()
    {
        dict = new Dictionary<ElementType, int>();
        for(int i = 0; i < (int)ElementType.COUNT; i++)
        {
            dict[(ElementType)i] = 0;
        }
    }

    public ElementResistances(Dictionary<ElementType, int> value)
    {
        dict = new Dictionary<ElementType, int>();
        foreach (KeyValuePair<ElementType, int> e in value)
        {
            dict[e.Key] = e.Value;
        }
    }

    public int GetResistance(ElementType e)
    {
        return dict[e];
    }

    public void SetResistance(ElementType e, int value)
    {
        dict[e] = value;
    }

    public int this[ElementType i]
    {
        get { return dict[i]; }
        set { dict[i] = value; }
    }

}