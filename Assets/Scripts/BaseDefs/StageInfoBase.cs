using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfoCollection
{
    [JsonProperty]
    private string idName;
    [JsonProperty]
    private int act;
    [JsonProperty]
    private int stage;
    [JsonProperty]
    private List<StageInfoBase> difficultyList;
}

public class StageInfoBase
{
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly DifficultyType difficulty;
    [JsonProperty]
    public readonly int monsterLevel;
    [JsonProperty]
    public readonly int baseExperience;
    [JsonProperty]
    public readonly List<WeightBase> equipmentDropList;
    [JsonProperty]
    public readonly List<WeightBase> archetypeDropList;
    [JsonProperty]
    public readonly List<ScalingBonusProperty> stageProperties;
    [JsonProperty]
    public readonly float expMultiplier;
    [JsonProperty]
    public readonly float equipmentDropRateMultiplier;
    [JsonProperty]
    public readonly float consumableDropRateMultiplier;
    [JsonProperty]
    public readonly List<EnemyWave> enemyWaves;
}

public class EnemyWave
{
    [JsonProperty]
    public readonly List<EnemyWaveItem> enemyList; // list of enemy types and number to spawn
    [JsonProperty]
    public readonly float delayBetweenSpawns;
    [JsonProperty]
    public readonly float delayUntilNextWave;
}

public class EnemyWaveItem
{
    [JsonProperty]
    public readonly string enemyName;
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public readonly RarityType enemyRarity;
    [JsonProperty]
    public readonly List<ScalingBonusProperty> bonusProperties;
    [JsonProperty]
    public readonly List<string> abilityOverrides;
    [JsonProperty]
    public readonly bool isBossOverride;
    [JsonProperty]
    public readonly int enemyCount;
    [JsonProperty]
    public readonly int spawnerIndex; // which spawn point to spawn from
}

public class WeightBase
{
    [JsonProperty]
    public string idName;
    [JsonProperty]
    public int weight;
}