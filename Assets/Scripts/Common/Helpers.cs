﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Helpers
{
    public const int AFFIX_STRING_SPACING = 1;
    public const string AFFIX_RANGE_COLOR = "#666";
    public static readonly Color UNIQUE_COLOR = new Color(1.0f, 0.5f, 0.2f);
    public static readonly Color EPIC_COLOR = new Color(0.86f, 0.35f, 0.86f);
    public static readonly Color UNCOMMON_COLOR = new Color(0.4f, 0.7f, 0.9f);
    public static readonly Color RARE_COLOR = new Color(1.0f, 0.9f, 0.25f);
    public static readonly Color NORMAL_COLOR = new Color(0.7f, 0.7f, 0.7f);
    public static readonly Color SELECTION_COLOR = new Color(0.2f, 0.9f, 0.82f);

    public static readonly Color STR_ARCHETYPE_COLOR = new Color(1f,0.4f,0.4f);
    public static readonly Color INT_ARCHETYPE_COLOR = new Color(0.23f, 0.7f, 1f);
    public static readonly Color AGI_ARCHETYPE_COLOR = new Color(0.23f, 0.86f, 0.23f);
    public static readonly Color WILL_ARCHETYPE_COLOR = new Color(0.96f, 0.9f, 0.26f);

    public static double SCALING_FACTOR = 1.042;
    public static double LEVEL_SCALING_FACTOR = 0.402;
    public static double ENEMY_SCALING = 1.012;

    private static List<BonusType> maxDamageTypes;
    private static Dictionary<string, BonusType> cachedDamageTypes;

    private static readonly Dictionary<ElementType, HashSet<BonusType>> conversionTypes = new Dictionary<ElementType, HashSet<BonusType>>();

    public static void GetGlobalAndFlatDamageTypes(ElementType element, AbilityType abilityType, AbilityShotType shotType, ICollection<GroupType> tags, HashSet<BonusType> min, HashSet<BonusType> max, HashSet<BonusType> multi)
    {
        if (abilityType == AbilityType.ATTACK || abilityType == AbilityType.SPELL)
        {
            switch (element)
            {
                case ElementType.PHYSICAL when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_PHYSICAL_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_PHYSICAL_DAMAGE_MAX);
                    break;

                case ElementType.PHYSICAL when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_PHYSICAL_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_PHYSICAL_DAMAGE_MAX);
                    break;

                case ElementType.FIRE when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_FIRE_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_FIRE_DAMAGE_MAX);
                    break;

                case ElementType.FIRE when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_FIRE_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_FIRE_DAMAGE_MAX);
                    break;

                case ElementType.COLD when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_COLD_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_COLD_DAMAGE_MAX);
                    break;

                case ElementType.COLD when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_COLD_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_COLD_DAMAGE_MAX);
                    break;

                case ElementType.LIGHTNING when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_LIGHTNING_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_LIGHTNING_DAMAGE_MAX);
                    break;

                case ElementType.LIGHTNING when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_LIGHTNING_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_LIGHTNING_DAMAGE_MAX);
                    break;

                case ElementType.EARTH when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_EARTH_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_EARTH_DAMAGE_MAX);
                    break;

                case ElementType.EARTH when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_EARTH_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_EARTH_DAMAGE_MAX);
                    break;

                case ElementType.DIVINE when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_DIVINE_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_DIVINE_DAMAGE_MAX);
                    break;

                case ElementType.DIVINE when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_DIVINE_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_DIVINE_DAMAGE_MAX);
                    break;

                case ElementType.VOID when abilityType == AbilityType.ATTACK:
                    min.Add(BonusType.ATTACK_VOID_DAMAGE_MIN);
                    max.Add(BonusType.ATTACK_VOID_DAMAGE_MAX);
                    break;

                case ElementType.VOID when abilityType == AbilityType.SPELL:
                    min.Add(BonusType.SPELL_VOID_DAMAGE_MIN);
                    max.Add(BonusType.SPELL_VOID_DAMAGE_MAX);
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
        }

        switch (shotType)
        {
            case AbilityShotType.PROJECTILE:
            case AbilityShotType.HITSCAN_SINGLE:
            case AbilityShotType.HITSCAN_MULTI:
            case AbilityShotType.PROJECTILE_NOVA:
                multi.Add(BonusType.PROJECTILE_DAMAGE);
                break;

            case AbilityShotType.ARC_AOE:
            case AbilityShotType.FORWARD_MOVING_ARC:
            case AbilityShotType.RADIAL_AOE:
            case AbilityShotType.FORWARD_MOVING_RADIAL:
            case AbilityShotType.NOVA_AOE:
            case AbilityShotType.NOVA_ARC_AOE:
            case AbilityShotType.LINEAR_AOE:
            case AbilityShotType.FORWARD_MOVING_LINEAR:
                multi.Add(BonusType.AREA_DAMAGE);
                break;
        }

        GetGlobalAndFlatDamageTypes(element, tags, min, max, multi);
    }

    public static void GetGlobalAndFlatDamageTypes(ElementType element, ICollection<GroupType> tags, HashSet<BonusType> min, HashSet<BonusType> max, HashSet<BonusType> multi)
    {
        if (!tags.Contains(GroupType.RETALIATION))
        {
            switch (element)
            {
                case ElementType.PHYSICAL:
                    min.Add(BonusType.GLOBAL_PHYSICAL_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_PHYSICAL_DAMAGE_MAX);
                    break;
                case ElementType.FIRE:
                    min.Add(BonusType.GLOBAL_FIRE_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_FIRE_DAMAGE_MAX);
                    break;
                case ElementType.COLD:
                    min.Add(BonusType.GLOBAL_COLD_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_COLD_DAMAGE_MAX);
                    break;
                case ElementType.LIGHTNING:
                    min.Add(BonusType.GLOBAL_LIGHTNING_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_LIGHTNING_DAMAGE_MAX);
                    break;
                case ElementType.EARTH:
                    min.Add(BonusType.GLOBAL_EARTH_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_EARTH_DAMAGE_MAX);
                    break;
                case ElementType.DIVINE:
                    min.Add(BonusType.GLOBAL_DIVINE_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_DIVINE_DAMAGE_MAX);
                    break;
                case ElementType.VOID:
                    min.Add(BonusType.GLOBAL_VOID_DAMAGE_MIN);
                    max.Add(BonusType.GLOBAL_VOID_DAMAGE_MAX);
                    break;
            }
        }

        multi.Add(BonusType.GLOBAL_DAMAGE);

        if (tags.Contains(GroupType.PROJECTILE))
        {
            multi.Add(BonusType.PROJECTILE_DAMAGE);
        }
        if (tags.Contains(GroupType.AREA))
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

        if (tags.Contains(GroupType.RETALIATION))
        {
            multi.Add(BonusType.RETALIATION_DAMAGE);
        }
    }

    public static HashSet<BonusType> GetMultiplierTypes(AbilityType abilityType, params ElementType[] elements)
    {
        HashSet<BonusType> bonusTypes = new HashSet<BonusType>();

        foreach (ElementType e in elements)
        {
            bonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), "GLOBAL_" + e.ToString() + "_DAMAGE"));
            if (abilityType == AbilityType.ATTACK || abilityType == AbilityType.SPELL)
            {
                bonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_" + e.ToString() + "_DAMAGE"));
                switch (e)
                {
                    case ElementType.FIRE:
                    case ElementType.COLD:
                    case ElementType.LIGHTNING:
                    case ElementType.EARTH:
                        bonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_ELEMENTAL_DAMAGE"));
                        bonusTypes.Add(BonusType.GLOBAL_ELEMENTAL_DAMAGE);
                        break;

                    case ElementType.DIVINE:
                    case ElementType.VOID:
                        bonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_PRIMORDIAL_DAMAGE"));
                        bonusTypes.Add(BonusType.GLOBAL_PRIMORDIAL_DAMAGE);
                        break;
                }
            }
        }

        return bonusTypes;
    }

    public static HashSet<BonusType> GetConversionTypes(ElementType element)
    {
        if (conversionTypes.ContainsKey(element))
            return conversionTypes[element];
        else
        {
            HashSet<BonusType> bonuses = new HashSet<BonusType>();

            foreach (ElementType e in Enum.GetValues(typeof(ElementType)))
            {
                if (e == element)
                    continue;
                if (Enum.TryParse(element.ToString() + "_CONVERT_" + e.ToString(), out BonusType b))
                    bonuses.Add(b);
            }

            conversionTypes[element] = bonuses;

            return bonuses;
        }
    }

    public static List<BonusType> GetWeaponDamageBonusTypes(AbilityType abilityType, ICollection<GroupType> tags, ElementType element)
    {
        List<BonusType> weaponBonuses = new List<BonusType>();

        foreach (GroupType groupTag in tags)
        {
            string weaponType = GetWeaponTypeString(groupTag);

            if (!string.IsNullOrEmpty(weaponType))
            {
                if (Enum.TryParse(weaponType + "_GLOBAL_DAMAGE", out BonusType globalDamage))
                    weaponBonuses.Add(globalDamage);

                if (abilityType == AbilityType.ATTACK)
                {
                    if (element == ElementType.PHYSICAL)
                        weaponBonuses.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_PHYSICAL_DAMAGE"));
                    else if (element == ElementType.VOID || element == ElementType.DIVINE)
                        weaponBonuses.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_PRIMORDIAL_DAMAGE"));
                    else
                        weaponBonuses.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_ELEMENTAL_DAMAGE"));
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    weaponBonuses.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_SPELL_DAMAGE"));
                }
            }
        }

        return weaponBonuses;
    }

    public static void GetWeaponSecondaryBonusTypes(AbilityType abilityType, ICollection<GroupType> tags,
        List<BonusType> critBonusTypes, List<BonusType> critDamageBonusTypes, List<BonusType> speedBonusTypes, List<BonusType> rangeBonusTypes)
    {
        foreach (GroupType groupTag in tags)
        {
            string weaponType = GetWeaponTypeString(groupTag);

            if (!string.IsNullOrEmpty(weaponType))
            {
                if (abilityType == AbilityType.ATTACK)
                {
                    critBonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_CRITICAL_CHANCE"));
                    critDamageBonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_CRITICAL_DAMAGE"));
                    speedBonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_ATTACK_SPEED"));
                }
                else if (abilityType == AbilityType.SPELL)
                {
                    speedBonusTypes.Add((BonusType)Enum.Parse(typeof(BonusType), weaponType + "_CAST_SPEED"));
                }
            }
        }
    }

    private static string GetWeaponTypeString(GroupType groupTag)
    {
        string weaponType;
        switch (groupTag)
        {
            case GroupType.ONE_HANDED_WEAPON:
                weaponType = "ONE_HANDED";
                break;

            case GroupType.TWO_HANDED_WEAPON:
                weaponType = "TWO_HANDED";
                break;

            case GroupType.ONE_HANDED_SWORD:
            case GroupType.TWO_HANDED_SWORD:
                weaponType = "SWORD";
                break;

            case GroupType.ONE_HANDED_AXE:
            case GroupType.TWO_HANDED_AXE:
                weaponType = "AXE";
                break;

            case GroupType.ONE_HANDED_MACE:
            case GroupType.TWO_HANDED_MACE:
                weaponType = "MACE";
                break;

            case GroupType.ONE_HANDED_GUN:
            case GroupType.TWO_HANDED_GUN:
                weaponType = "GUN";
                break;

            case GroupType.BOW:
            case GroupType.CROSSBOW:
                weaponType = "BOW";
                break;

            case GroupType.WAND:
            case GroupType.STAFF:
            case GroupType.SPEAR:
            case GroupType.FIST:
            case GroupType.MELEE_WEAPON:
            case GroupType.RANGED_WEAPON:
                weaponType = groupTag.ToString();
                break;

            default:
                weaponType = string.Empty;
                break;
        }

        return weaponType;
    }

    public static Vector3 ReturnCenterOfCell(Vector3 v)
    {
        return new Vector3((float)Math.Round(v.x * 2f) / 2f, (float)Math.Round(v.y * 2f) / 2f, v.z);
    }

    public static bool RollChance(float chance)
    {
        if (chance <= 0f)
        {
            return false;
        }

        if (chance >= 1f)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 1f) < chance ? true : false;
    }

    public static double GetEnemyHealthScaling(double level)
    {
        // formula
        // (Scaling*(EnemyScaling))^(level*1.1 - 23) * (level*levelFactor*5) + level*2
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level * 1.1 - 23) * level * LEVEL_SCALING_FACTOR * 5 + level * 2;

        return enemyFactor * 15;
    }

    public static double GetEnemyDamageScaling(double level)
    {
        // formula
        // (Scaling*(EnemyScaling))^(level/1.5 - 22) * (level*levelFactor) + level*2
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level / 1.5 - 23) * level * LEVEL_SCALING_FACTOR + level * 2;

        return enemyFactor * 5;
    }

    public static double GetEnemyAccuracyScaling(double level)
    {
        // formula
        // (Scaling*(EnemyScaling))^(level/1.5 - 22) * (level*levelFactor) + level*2
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level / 1.4 - 20) * level * LEVEL_SCALING_FACTOR + level * 4;

        return enemyFactor * 4;
    }

    public static int GetRequiredExperience(int level)
    {
        double exp = Math.Pow(SCALING_FACTOR, (level - 1) * 1.333 - 30) * (level - 1) * 500;
        return (int)exp;
    }

    public static Color GetArchetypeStatColor(ArchetypeBase archetype)
    {
        List<float> growths = new List<float>() { archetype.strengthGrowth, archetype.intelligenceGrowth, archetype.agilityGrowth, archetype.willGrowth };
        int sameCount = 0, sameGrowthIndex = 0, highestIndex = 0, secondHighestIndex = 0;
        float highest = 0, secondHighest = 0, sum = 0;
        for (int i = 0; i < growths.Count; i++)
        {
            if (growths[i] > highest)
            {
                highest = growths[i];
                highestIndex = i;
            }
            sum += growths[i];
        }

        for (int j = 0; j < growths.Count; j++)
        {
            if (j == highestIndex)
            {
                continue;
            }
            else if (growths[j] == highest)
            {
                sameCount++;
                sameGrowthIndex = j;
            }
            else if (growths[j] > secondHighest)
            {
                secondHighest = growths[j];
                secondHighestIndex = j;
            }
        }

        if (sameCount >= 2)
            return NORMAL_COLOR;
        else if (sameCount == 1)
            return Color.Lerp(GetColorFromStatIndex(highestIndex), GetColorFromStatIndex(sameGrowthIndex), 0.5f);
        else
            return Color.Lerp(GetColorFromStatIndex(highestIndex), Color.Lerp(GetColorFromStatIndex(highestIndex), GetColorFromStatIndex(secondHighestIndex), 0.5f), 0.05f / (highest - growths[secondHighestIndex]));
    }

    public static Color GetColorFromStatIndex(int index)
    {
        switch (index)
        {
            case 0:
                return STR_ARCHETYPE_COLOR;

            case 1:
                return INT_ARCHETYPE_COLOR;

            case 2:
                return AGI_ARCHETYPE_COLOR;

            case 3:
                return WILL_ARCHETYPE_COLOR;

            default:
                return NORMAL_COLOR;
        }
    }

    public static Color ReturnRarityColor(RarityType rarity)
    {
        switch (rarity)
        {
            case RarityType.UNIQUE:
                return UNIQUE_COLOR;

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

    public static Vector3 ReturnTilePosition(Tilemap tilemap, Vector3 position, int z = 0)
    {
        Vector3Int cellPos = tilemap.WorldToCell(position);
        Vector3 returnVal = tilemap.GetCellCenterWorld(cellPos);
        returnVal.z = z;
        return returnVal;
    }

    public static Vector3Int ReturnTilePosition_Int(Tilemap tilemap, Vector3 position)
    {
        Vector3Int cellPos = tilemap.WorldToCell(position);
        return cellPos;
    }

    public static WeightList<string> CreateWeightListFromWeightBases(List<WeightBase> weightBases)
    {
        WeightList<string> ret = new WeightList<string>();
        foreach (WeightBase weightBase in weightBases)
        {
            ret.Add(weightBase.idName, weightBase.weight);
        }
        return ret;
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
        if (Sum == 0)
            return default;

        int weight = UnityEngine.Random.Range(1, Sum + 1);
        foreach (WeightListItem<T> x in list)
        {
            weight -= x.weight;
            if (weight <= 0)
                return x.item;
        }
        Debug.Log("Did not return proper item. Error in sum or list?");
        return list[UnityEngine.Random.Range(0, list.Count)].item;
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

    public IEnumerator<WeightListItem<T>> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public class WeightListItem<T2>
    {
        public T2 item;
        public int weight;

        public WeightListItem(T2 i, int w)
        {
            item = i;
            weight = w;
        }
    }
}