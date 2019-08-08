using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfoBase
{
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly int act;
    [JsonProperty]
    public readonly int stage;
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
    public readonly List<string> stageProperties;
    [JsonProperty]
    public readonly float expMultiplier;
    [JsonProperty]
    public readonly int equipmentDropCountMin;
    [JsonProperty]
    public readonly int equipmentDropCountMax;
    [JsonProperty]
    public readonly int consumableDropCountMin;
    [JsonProperty]
    public readonly int consumableDropCountMax;
    [JsonProperty]
    public readonly List<EnemyWave> enemyWaves;
    [JsonProperty]
    public readonly int sceneAct;
    [JsonProperty]
    public readonly int sceneStage;

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
    public readonly List<string> bonusProperties;
    [JsonProperty]
    public readonly List<string> abilityOverrides;
    [JsonProperty]
    public readonly bool isBossOverride;
    [JsonProperty]
    public readonly int enemyCount;
    [JsonProperty]
    public readonly int spawnerIndex; // which spawn point to spawn from
    [JsonProperty]
    public readonly int goalIndex; // which goal to head toward
}

public class WeightBase
{
    [JsonProperty]
    public readonly string idName;
    [JsonProperty]
    public readonly int weight;
}