using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WaveManager : MonoBehaviour {

    private List<Spawner> spawnerList;
    private List<Goal> goalList;
    public List<EnemyWave> waves;
    public List<EnemyActor> currentEnemyList;
    private int currentWave;
    private bool clearedAll;
    public int enemiesSpawned;

    // Use this for initialization
    void Start () {
        enemiesSpawned = 0;
        this.SpawnWave();
	}

    public void SpawnWave()
    {
        StartCoroutine(SpawnWaveCo(currentWave));
        currentWave++;
    }

    private IEnumerator SpawnWaveCo(int currentWave)
    {
        EnemyWave waveToSpawn = waves[currentWave];
        for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
        {
            for (int j = 0; j < waveToSpawn.enemyList[i].enemyCount; j++)
            {
                Spawner spawner = SpawnerList[waveToSpawn.enemyList[i].spawnerIndex];
                yield return new WaitForSeconds(waveToSpawn.delayBetweenSpawns);
                var enemy = Instantiate(ResourceManager.Instance.EnemyPrefab, spawner.transform.position, spawner.transform.rotation);
                enemy.GetComponent<EnemyActor>().ParentSpawner = spawner;
                currentEnemyList.Add(enemy.GetComponent<EnemyActor>());
                enemiesSpawned++;
            }
        }
        yield break;
    }

    public List<Goal> GoalList
    {
        get
        {
            if (goalList == null)
            {
                goalList = new List<Goal>();
                foreach (var g in FindObjectsOfType<Goal>())
                {
                    g.transform.position = Helpers.ReturnCenterOfCell(g.transform.position);
                    goalList.Add(g);
                }
                goalList = goalList.OrderBy(x => x.goalIndex).ToList();
            }
            return goalList;
        }
    }
    public List<Spawner> SpawnerList
    {
        get
        {
            if (spawnerList == null)
            {
                spawnerList = new List<Spawner>();
                foreach (var s in FindObjectsOfType<Spawner>())
                {
                    s.transform.position = Helpers.ReturnCenterOfCell(s.transform.position);
                    spawnerList.Add(s);
                }
                spawnerList = spawnerList.OrderBy(x => x.spawnerIndex).ToList();
            }
            return spawnerList;
        }
    }
}
