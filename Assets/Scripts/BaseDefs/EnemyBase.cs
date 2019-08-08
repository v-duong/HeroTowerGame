using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class EnemyBase
{
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly int level;
    [JsonProperty]
    public readonly int experience;
    [JsonProperty]
    public readonly bool isBoss;
    [JsonProperty]
    public readonly float healthScaling;
    [JsonProperty]
    public readonly float movementSpeed;
    [JsonProperty]
    public readonly int[] resistances;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly EnemyType enemyType;
    [JsonProperty]
    public readonly List<EnemyAbilityBase> abilitiesList;
    [JsonProperty]
    public readonly string spriteName;
    [JsonProperty]
    public readonly float attackTargetRange;
    [JsonProperty]
    public readonly float attackSpeed;
    [JsonProperty]
    public readonly float attackDamageMinMultiplier;
    [JsonProperty]
    public readonly float attackDamageMaxMultiplier;
    [JsonProperty]
    public readonly float attackCriticalChance;

    public class EnemyAbilityBase
    {
        [JsonProperty]
        public string abilityName;
        [JsonProperty]
        public float damageMultiplier;
        [JsonProperty]
        public float attackPerSecMultiplier;
    }
}

public enum EnemyType
{
    NON_ATTACKER,
    TARGET_ATTACKER,
    HIT_AND_RUN,
    AURA_USER,
    DEBUFFER
}

