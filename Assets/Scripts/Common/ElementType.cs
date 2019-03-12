using System.Collections;
using System.Collections.Generic;
using System;

public enum ElementType
{
    NONE, //none/physical
    FIRE,
    COLD,
    LIGHTNING,
    EARTH,
    DIVINE,
    VOID,
}

public class ElementResistances
{
    private readonly Dictionary<ElementType, int> dict;

    public ElementResistances()
    {
        dict = new Dictionary<ElementType, int>();
        var elementEnums = Enum.GetValues(typeof(ElementType));
        foreach (ElementType element in elementEnums)
        {
            dict.Add(element, 0);
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
}