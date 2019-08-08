using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private List<Spawner> spawnerList;
    private List<Goal> goalList;
    private Coroutine waveCoroutine;
    private StageInfoBase stageInfo;

    public IReadOnlyList<EnemyWave> Waves { get; private set; }
    public List<EnemyActor> currentEnemyList;
    public List<HeroData> activeHeroes = new List<HeroData>();

    public int enemiesSpawned;
    public bool startedSpawn = false;
    public bool finishedSpawn = false;
    public int selectedTeam;
    public bool battleEnded = false;
    public int playerHealth = 20;
    public int stageLevel = 1;

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
            this.SpawnWave(0);
        }
        if (!battleEnded)
        {
            if (playerHealth <= 0)
            {
                EndBattle(false);
                foreach (EnemyActor actor in currentEnemyList)
                {
                    actor.DisableActor();
                }
            }
            if (finishedSpawn && currentEnemyList.Count == 0)
            {
                int consumableDrops = Random.Range(stageInfo.consumableDropCountMin, stageInfo.consumableDropCountMax + 1);
                for (int i = 0; i < consumableDrops; i++)
                    GameManager.Instance.AddRandomConsumableToInventory();

                int equipmentDrops = Random.Range(stageInfo.equipmentDropCountMin, stageInfo.equipmentDropCountMax + 1);
                if (stageInfo.equipmentDropList.Count == 0)
                {
                    for (int i = 0; i < equipmentDrops; i++)
                        GameManager.Instance.PlayerStats.AddEquipmentToInventory(Equipment.CreateRandomEquipment(stageLevel));
                }
                else
                {
                    WeightList<string> weightList = Helpers.CreateWeightListFromWeightBases(stageInfo.equipmentDropList);
                    for (int i = 0; i < equipmentDrops; i++)
                    {
                        EquipmentBase equipmentBase = ResourceManager.Instance.GetEquipmentBase(weightList.ReturnWeightedRandom());
                        GameManager.Instance.PlayerStats.AddEquipmentToInventory(Equipment.CreateEquipmentFromBase(equipmentBase, stageLevel));
                    }
                }

                EndBattle(true);
            }
        }
    }

    public void SetStageBase(StageInfoBase stage)
    {
        stageInfo = stage;
        Waves = stage.enemyWaves;
        stageLevel = stage.monsterLevel;
    }

    private void EndBattle(bool victory)
    {
        StopAllCoroutines();
        battleEnded = true;
        if (victory)
        {
            Debug.Log("VIC");
            foreach (HeroData hero in GameManager.Instance.inBattleHeroes)
            {
                hero.AddExperience(5000);
            }
        }
        else
        {
            Debug.Log("FAIL");
        }
    }

    public void SpawnWave(int wave)
    {
        waveCoroutine = StartCoroutine(SpawnWaveCo(wave));
        //currentWave++;
    }

    private IEnumerator SpawnWaveCo(int currentWave)
    {
        EnemyWave waveToSpawn = Waves[currentWave];
        EnemyBase enemyBase;
        RarityType rarity;
        Dictionary<BonusType, StatBonus> bonuses = new Dictionary<BonusType, StatBonus>();
        for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
        {
            enemyBase = ResourceManager.Instance.GetEnemyBase(waveToSpawn.enemyList[i].enemyName);
            rarity = waveToSpawn.enemyList[i].enemyRarity;

            bonuses.Clear();
            foreach (string affixName in waveToSpawn.enemyList[i].bonusProperties)
            {
                AffixBase affixBase = ResourceManager.Instance.GetAffixBase(affixName, AffixType.MONSTERMOD);
                foreach (AffixBonusProperty prop in affixBase.affixBonuses)
                {
                    StatBonus bonus;
                    if (bonuses.ContainsKey(prop.bonusType))
                        bonus = bonuses[prop.bonusType];
                    else
                    {
                        bonus = new StatBonus();
                        bonuses.Add(prop.bonusType, bonus);
                    }
                    bonus.AddBonus(prop.modifyType, prop.minValue);
                }
            }

            for (int j = 0; j < waveToSpawn.enemyList[i].enemyCount; j++)
            {
                Spawner spawner = SpawnerList[waveToSpawn.enemyList[i].spawnerIndex];
                yield return new WaitForSeconds(waveToSpawn.delayBetweenSpawns);

                EnemyActor enemy = Instantiate(ResourceManager.Instance.EnemyPrefab, spawner.transform.position, spawner.transform.rotation).GetComponent<EnemyActor>();
                enemy.ParentSpawner = spawner;
                enemy.SetBase(enemyBase, rarity, stageInfo.monsterLevel);
                enemy.Data.SetMobBonuses(bonuses);
                if (enemyBase.isBoss || waveToSpawn.enemyList[i].isBossOverride)
                    enemy.isBoss = true;

                currentEnemyList.Add(enemy.GetComponent<EnemyActor>());
                enemiesSpawned++;
            }
        }
        if (currentWave + 1 < Waves.Count)
        {
            yield return new WaitForSeconds(waveToSpawn.delayUntilNextWave);
            waveCoroutine = StartCoroutine(SpawnWaveCo(currentWave + 1));
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