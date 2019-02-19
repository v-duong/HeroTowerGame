using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Helpers {
    public static Color EPIC_COLOR = new Color(1,0,1);
    public static Color UNCOMMON_COLOR = new Color(0.2f, 0.4f, 0.8f);
    public static Color RARE_COLOR = new Color(0.8f, 0.8f, 0.25f);
    public static Color NORMAL_COLOR = new Color(0.7f, 0.7f, 0.7f);

    public static Vector3 ReturnCenterOfCell(Vector3 v)
    {
        return new Vector3((float) Math.Round(v.x * 2f) / 2f, (float)Math.Round(v.y * 2f) / 2f, v.z);
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

    public static T ReturnWeightedRandom<T>(List<WeightListItem<T>> list, int sum)
    {
        int weight = UnityEngine.Random.Range(1, sum + 1);
        foreach(WeightListItem<T> x in list)
        {
            weight -= x.weight;
            if (weight <= 0)
                return x.item;
        }
        return default(T);
    }

    public class WeightListItem<T> {
        public T item;
        public int weight;

        public WeightListItem(T i, int w)
        {
            item = i;
            weight = w;
        }
    }

    
}

