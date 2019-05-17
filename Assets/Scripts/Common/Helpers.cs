﻿using System;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static readonly Color EPIC_COLOR = new Color(0.86f, 0.35f, 0.86f);
    public static readonly Color UNCOMMON_COLOR = new Color(0.4f, 0.7f, 0.9f);
    public static readonly Color RARE_COLOR = new Color(1.0f, 0.9f, 0.25f);
    public static readonly Color NORMAL_COLOR = new Color(0.7f, 0.7f, 0.7f);

    private static List<BonusType> maxDamageTypes;
    private static List<BonusType> damageTypes;

    public static void GetDamageTypes(ElementType element, AbilityType abilityType, AbilityShotType shotType, List<GroupType> tags, List<BonusType> min, List<BonusType> max, List<BonusType> multi)
    {
        switch (element)
        {
            case ElementType.PHYSICAL:
                min.Add(BonusType.GLOBAL_PHYSICAL_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_PHYSICAL_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_PHYSICAL_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_PHYSICAL_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_PHYSICAL_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_PHYSICAL_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_PHYSICAL_DAMAGE_MAX);
                }
                break;

            case ElementType.FIRE:
                min.Add(BonusType.GLOBAL_FIRE_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_FIRE_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_FIRE_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_FIRE_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_FIRE_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_FIRE_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_FIRE_DAMAGE_MAX);
                }
                break;

            case ElementType.COLD:
                min.Add(BonusType.GLOBAL_COLD_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_COLD_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_COLD_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_COLD_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_COLD_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_COLD_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_COLD_DAMAGE_MAX);
                }
                break;

            case ElementType.LIGHTNING:
                min.Add(BonusType.GLOBAL_LIGHTNING_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_LIGHTNING_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_LIGHTNING_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_LIGHTNING_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_LIGHTNING_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_LIGHTNING_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_LIGHTNING_DAMAGE_MAX);
                }
                break;

            case ElementType.EARTH:
                min.Add(BonusType.GLOBAL_EARTH_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_EARTH_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_EARTH_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_EARTH_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_EARTH_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_EARTH_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_EARTH_DAMAGE_MAX);
                }
                break;

            case ElementType.DIVINE:
                min.Add(BonusType.GLOBAL_DIVINE_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_DIVINE_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_DIVINE_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_DIVINE_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_DIVINE_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_DIVINE_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_DIVINE_DAMAGE_MAX);
                }
                break;

            case ElementType.VOID:
                min.Add(BonusType.GLOBAL_VOID_DAMAGE_MIN);
                max.Add(BonusType.GLOBAL_VOID_DAMAGE_MAX);
                multi.Add(BonusType.GLOBAL_VOID_DAMAGE);
                if (abilityType == AbilityType.ATTACK)
                {
                    min.Add(BonusType.ATTACK_VOID_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_VOID_DAMAGE_MAX);
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    min.Add(BonusType.SPELL_VOID_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_VOID_DAMAGE_MAX);
                }
                break;

            default:
                break;
        }

        if (abilityType == AbilityType.ATTACK)
        {
            multi.Add(BonusType.ATTACK_DAMAGE);
        }
        else if (abilityType == AbilityType.SPELL)
        {
            multi.Add(BonusType.SPELL_DAMAGE);
        }

        if (shotType == AbilityShotType.PROJECTILE)
        {
            multi.Add(BonusType.PROJECTILE_DAMAGE);
        }
        else if (shotType == AbilityShotType.HITSCAN_SINGLE)
        {
            multi.Add(BonusType.PROJECTILE_DAMAGE);
        }
        else
        {
            multi.Add(BonusType.AREA_DAMAGE);
        }

        if (tags.Contains(GroupType.MELEE_ATTACK))
        {
            multi.Add(BonusType.MELEE_ATTACK_DAMAGE);
        }
        else if (tags.Contains(GroupType.RANGED_ATTACK))
        {
            multi.Add(BonusType.RANGED_ATTACK_DAMAGE);
        }
    }

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
        Debug.Log("Did not return proper item. Error in sum or list");
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

