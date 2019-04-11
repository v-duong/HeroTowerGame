using System;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static readonly Color EPIC_COLOR = new Color(0.86f, 0.35f, 0.86f);
    public static readonly Color UNCOMMON_COLOR = new Color(0.4f, 0.7f, 0.9f);
    public static readonly Color RARE_COLOR = new Color(1.0f, 0.9f, 0.25f);
    public static readonly Color NORMAL_COLOR = new Color(0.7f, 0.7f, 0.7f);

    public static Vector3 ReturnCenterOfCell(Vector3 v)
    {
        return new Vector3((float)Math.Round(v.x * 2f) / 2f, (float)Math.Round(v.y * 2f) / 2f, v.z);
    }

    public static Color ReturnRarityColor(RarityType rarity)
    {
        switch (rarity)
        {
            case RarityType.EPIC:
                return EPIC_COLOR;

            case RarityType.UNCOMMON:
                return UNCOMMON_COLOR;

            case RarityType.RARE:
                return RARE_COLOR;

            case RarityType.NORMAL:
                return NORMAL_COLOR;

            default:
                return Color.black;
        }
    }
}

public class WeightListItem<T>
{
    public T item;
    public int weight;

    public WeightListItem(T i, int w)
    {
        item = i;
        weight = w;
    }
}

public class WeightList<T>
{
    private List<WeightListItem<T>> list;
    public int Sum { get; private set; }
    public int Count { get => list.Count; }

    public WeightList()
    {
        list = new List<WeightListItem<T>>();
    }

    public T ReturnWeightedRandom()
    {
        int weight = UnityEngine.Random.Range(1, Sum + 1);
        foreach (WeightListItem<T> x in list)
        {
            weight -= x.weight;
            if (weight <= 0)
                return x.item;
        }
        Debug.Log("Did not return proper item. Error in sum?");
        return default;
    }

    public void Add(T item, int value)
    {
        list.Add(new WeightListItem<T>(item, value));
        Sum += value;
    }

    public void Clear()
    {
        list.Clear();
        Sum = 0;
    }
}