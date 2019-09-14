using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Helpers
{
    public static readonly Color UNIQUE_COLOR = new Color(1.0f, 0.5f, 0.2f);
    public static readonly Color EPIC_COLOR = new Color(0.86f, 0.35f, 0.86f);
    public static readonly Color UNCOMMON_COLOR = new Color(0.4f, 0.7f, 0.9f);
    public static readonly Color RARE_COLOR = new Color(1.0f, 0.9f, 0.25f);
    public static readonly Color NORMAL_COLOR = new Color(0.7f, 0.7f, 0.7f);

    public static double SCALING_FACTOR = 1.042;
    public static double LEVEL_SCALING_FACTOR = 0.402;
    public static double ENEMY_SCALING = 1.012;

    private static List<BonusType> maxDamageTypes;
    private static List<BonusType> damageTypes;

    public static void GetDamageTypes(ElementType element, AbilityType abilityType, AbilityShotType shotType, ICollection<GroupType> tags, HashSet<BonusType> min, HashSet<BonusType> max, HashSet<BonusType> multi)
    {
        min.Add((BonusType)Enum.Parse(typeof(BonusType), "GLOBAL_" + element.ToString() + "_DAMAGE_MIN"));
        max.Add((BonusType)Enum.Parse(typeof(BonusType), "GLOBAL_" + element.ToString() + "_DAMAGE_MAX"));
        multi.Add((BonusType)Enum.Parse(typeof(BonusType), "GLOBAL_" + element.ToString() + "_DAMAGE"));

        if (abilityType == AbilityType.ATTACK || abilityType == AbilityType.SPELL)
        {
            min.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_" + element.ToString() + "_DAMAGE_MIN"));
            max.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_" + element.ToString() + "_DAMAGE_MAX"));
            multi.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_" + element.ToString() + "_DAMAGE"));
            switch (element)
            {
                case ElementType.FIRE:
                case ElementType.COLD:
                case ElementType.LIGHTNING:
                case ElementType.EARTH:
                    multi.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_ELEMENTAL_DAMAGE"));
                    multi.Add(BonusType.GLOBAL_ELEMENTAL_DAMAGE);
                    break;

                case ElementType.DIVINE:
                case ElementType.VOID:
                    multi.Add((BonusType)Enum.Parse(typeof(BonusType), abilityType.ToString() + "_PRIMORDIAL_DAMAGE"));
                    multi.Add(BonusType.GLOBAL_PRIMORDIAL_DAMAGE);
                    break;
            }
        }

        multi.Add(BonusType.GLOBAL_DAMAGE);

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

            case GroupType.GRIMOIRE:
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
        // (Scaling*(EnemyScaling))^(level/1.5 - 22) * (level*levelFactor) + level*2
        double enemyFactor = Math.Pow(SCALING_FACTOR * ENEMY_SCALING, level / 1.5 - 23) * level * LEVEL_SCALING_FACTOR + level * 2;

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

    private class WeightListItem<T2>
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