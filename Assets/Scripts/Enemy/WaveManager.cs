using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WaveManager : MonoBehaviour {
    private List<Spawner> m_spawnerList;
    private List<Goal> m_goalList;
    public List<EnemyWave> waves;
    public List<EnemyActor> currentEnemyList;
    private int m_currentWave;
    private bool m_clearedAll;
    public int enemiesSpawned;

	// Use this for initialization
	void Start () {
        enemiesSpawned = 0;
        this.SpawnWave();
	}

    public void SpawnWave()
    {
        StartCoroutine(SpawnWaveCo(m_currentWave));
        m_currentWave++;
    }

    private IEnumerator SpawnWaveCo(int currentWave)
    {
        EnemyWave waveToSpawn = waves[currentWave];
        for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
        {
            for (int j = 0; j < waveToSpawn.enemyList[i].enemyCount; j++)
            {
                Spawner spawner = SpawnerList[waveToSpawn.spawnerIndex];
                yield return new WaitForSeconds(waveToSpawn.delayBetweenSpawns);
                var enemy = Instantiate(waveToSpawn.enemyList[i].enemyType, spawner.transform.position, spawner.transform.rotation);
                enemy.ParentSpawner = spawner;
                currentEnemyList.Add(enemy);
                enemiesSpawned++;
            }
        }
        yield break;
    }

    public List<Goal> GoalList
    {
        get
        {
            if (m_goalList == null)
            {
                m_goalList = new List<Goal>();
                foreach (var g in FindObjectsOfType<Goal>())
                {
                    g.transform.position = Helpers.ReturnCenterOfCell(g.transform.position);
                    m_goalList.Add(g);
                }
                m_goalList = m_goalList.OrderBy(x => x.goalIndex).ToList();
            }
            return m_goalList;
        }
    }
    public List<Spawner> SpawnerList
    {
        get
        {
            if (m_spawnerList == null)
            {
                m_spawnerList = new List<Spawner>();
                foreach (var s in FindObjectsOfType<Spawner>())
                {
                    s.transform.position = Helpers.ReturnCenterOfCell(s.transform.position);
                    m_spawnerList.Add(s);
                }
                m_spawnerList = m_spawnerList.OrderBy(x => x.spawnerIndex).ToList();
            }
            return m_spawnerList;
        }
    }
}
