using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EnemyWave {
    public List<EnemyWaveDefinition> enemyList; // list of enemy types and number to spawn
    public float delayBetweenSpawns;
    public float delayUntilNextWave = 15f;
    public int spawnerIndex; // which spawn point to spawn from
}
[Serializable]
public class EnemyWaveDefinition
{
    public EnemyActor enemyType;
    public int enemyCount;
}