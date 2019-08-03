using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private List<Spawner> spawnerList;
    private List<Goal> goalList;
    public List<EnemyWave> Waves { get; private set; }
    public List<EnemyActor> currentEnemyList;
    private int currentWave = 0;
    public int enemiesSpawned;
    public bool startedSpawn = false;
    public bool finishedSpawn = false;
    public List<HeroData> activeHeroes = new List<HeroData>();
    public int selectedTeam;
    public bool battleEnded = false;

    // Use this for initialization
    private void Start()
    {
        enemiesSpawned = 0;
    }

    private void Update()
    {
        if (!startedSpawn && Waves != null)
        {
            startedSpawn = true;
            this.SpawnWave();
        }
        if (finishedSpawn && currentEnemyList.Count == 0 && !battleEnded)
        {
            EndBattle(true);
            battleEnded = true;
        }
    }

    public void SetWaves(List<EnemyWave> waves)
    {
        this.Waves = waves;
    }

    private void EndBattle(bool victory)
    {
        if (victory)
        {
            Debug.Log("VIC");
            foreach (HeroData hero in GameManager.Instance.inBattleHeroes)
            {
                hero.AddExperience(5000);
            }
        }
    }

    public void SpawnWave()
    {
        StartCoroutine(SpawnWaveCo(currentWave));
        //currentWave++;
    }

    private IEnumerator SpawnWaveCo(int currentWave)
    {
        EnemyWave waveToSpawn = Waves[currentWave];
        EnemyBase enemyBase;
        for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
        {
            enemyBase = ResourceManager.Instance.GetEnemyBase(waveToSpawn.enemyList[i].enemyName);
            for (int j = 0; j < waveToSpawn.enemyList[i].enemyCount; j++)
            {
                Spawner spawner = SpawnerList[waveToSpawn.enemyList[i].spawnerIndex];
                yield return new WaitForSeconds(waveToSpawn.delayBetweenSpawns);

                var enemy = Instantiate(ResourceManager.Instance.EnemyPrefab, spawner.transform.position, spawner.transform.rotation);
                enemy.GetComponent<EnemyActor>().ParentSpawner = spawner;
                enemy.GetComponent<EnemyActor>().SetBase(enemyBase);

                currentEnemyList.Add(enemy.GetComponent<EnemyActor>());
                enemiesSpawned++;
            }
        }
        if (currentWave + 1 < Waves.Count)
        {
            yield return new WaitForSeconds(waveToSpawn.delayUntilNextWave);
            StartCoroutine(SpawnWaveCo(currentWave + 1));
        }
        else
        {
            finishedSpawn = true;
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
                    spawnerList.Add(s);
                }
                spawnerList = spawnerList.OrderBy(x => x.spawnerIndex).ToList();
            }
            return spawnerList;
        }
    }
}