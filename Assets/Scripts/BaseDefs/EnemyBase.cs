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

    public class EnemyAbilityBase
    {
        [JsonProperty]
        public string abilityName;
        [JsonProperty]
        public float damageMultiplier;
        [JsonProperty]
        public float cooldownMultiplier;
    }
}

public enum EnemyType
{
    NON_ATTACKER,
    ATTACKER,
    HIT_AND_RUN
}

