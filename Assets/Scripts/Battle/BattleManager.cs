﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private List<Spawner> spawnerList;
    private List<Goal> goalList;
    private int spawnCoroutinesRunning = 0;
    private Coroutine waveCoroutine;
    private StageInfoBase stageInfo;

    public EnemyPool EnemyPool { get; private set; }

    public ProjectilePool ProjectilePool { get; private set; }
    public ProjectilePool BoxProjectilePool { get; private set; }

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
    private int extraWaves = 0;

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
            SpawnWave(0);
        }
        if (!battleEnded)
        {
            if (playerHealth <= 0)
            {
                EndBattle(victory: false);
            }
            else if (finishedSpawn && currentEnemyList.Count == 0 && spawnCoroutinesRunning == 0)
            {
                EndBattle(victory: true);
            }
        }
    }

    public void SetStageBase(StageInfoBase stage)
    {
        stageInfo = stage;
        Waves = stage.enemyWaves;
        stageLevel = stage.monsterLevel;
        EnemyPool = new EnemyPool(ResourceManager.Instance.EnemyPrefab);
    }

    public void InitializeProjectilePool()
    {
        ProjectilePool = new ProjectilePool(GameManager.Instance.projectilePrefab);
        BoxProjectilePool = new ProjectilePool(GameManager.Instance.boxProjectilePrefab);
    }

    private void EndBattle(bool victory)
    {
        StageManager.Instance.BattleManager.ProjectilePool.ReturnAll();
        foreach (EnemyActor enemy in currentEnemyList)
        {
            enemy.DisableActor();
            EnemyPool.ReturnToPool(enemy);
        }
        enemiesSpawned = 0;
        currentEnemyList.Clear();
        StopAllCoroutines();
        battleEnded = true;
        GameManager.Instance.isInBattle = false;
        BattleEndWindow battleEndWindow = UIManager.Instance.BattleUICanvas.GetComponentInChildren<BattleEndWindow>(true);

        if (victory)
        {
            Debug.Log("VIC");

            battleEndWindow.ShowVictoryWindow();

            int gainedExp = (int)(stageInfo.baseExperience * (stageInfo.expMultiplier + (extraWaves / 5)));

            foreach (HeroData hero in GameManager.Instance.inBattleHeroes)
            {
                hero.AddExperience(gainedExp);
            }
            GameManager.Instance.PlayerStats.ModifyExpStock((int)(gainedExp * 0.25f));

            battleEndWindow.AddToBodyText("Experience +" + gainedExp + "\nStocked Experience +" + (gainedExp*0.25f) + "\n");

            // Get Consumables
            int consumableDrops = Random.Range(stageInfo.consumableDropCountMin, stageInfo.consumableDropCountMax + 1);
            Dictionary<ConsumableType, int> consumablesCount = new Dictionary<ConsumableType, int>();
            for (int i = 0; i < consumableDrops; i++)
            {
                ConsumableType consumable = GameManager.Instance.GetRandomConsumable();
                GameManager.Instance.PlayerStats.consumables[consumable]++;
                if (!consumablesCount.ContainsKey(consumable))
                    consumablesCount.Add(consumable,0);
                consumablesCount[consumable]++;
            }

            foreach(KeyValuePair<ConsumableType, int> keyValue in consumablesCount)
            {
                battleEndWindow.AddToBodyText(keyValue.Key.ToString() + " +" + keyValue.Value + "\n");
            }

            //Get Equipment
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
                    string baseId = weightList.ReturnWeightedRandom();
                    EquipmentBase equipmentBase = ResourceManager.Instance.GetEquipmentBase(baseId);
                    GameManager.Instance.PlayerStats.AddEquipmentToInventory(Equipment.CreateEquipmentFromBase(equipmentBase, stageLevel));
                }
            }

            //Get Archetype
            if (stageInfo.archetypeDropList.Count != 0)
            {
                WeightList<string> weightList = Helpers.CreateWeightListFromWeightBases(stageInfo.archetypeDropList);
                string baseId = weightList.ReturnWeightedRandom();
                ArchetypeBase archetypeBase = ResourceManager.Instance.GetArchetypeBase(baseId);
                GameManager.Instance.PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateArchetypeItem(archetypeBase, stageLevel));
            }
        }
        else
        {
            Debug.Log("FAIL");
            battleEndWindow.ShowLoseWindow();
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

        for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
        {
            StartCoroutine(SpawnEnemyList(waveToSpawn.enemyList[i], waveToSpawn.delayBetweenSpawns));
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

    private IEnumerator SpawnEnemyList(EnemyWaveItem enemyWaveItem, float delayBetween)
    {
        spawnCoroutinesRunning++;

        Dictionary<BonusType, StatBonus> bonuses = new Dictionary<BonusType, StatBonus>();
        EnemyBase enemyBase = ResourceManager.Instance.GetEnemyBase(enemyWaveItem.enemyName);
        RarityType rarity = enemyWaveItem.enemyRarity;

        foreach (string affixName in enemyWaveItem.bonusProperties)
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

        for (int j = 0; j < enemyWaveItem.enemyCount; j++)
        {
            Spawner spawner = SpawnerList[enemyWaveItem.spawnerIndex];

            EnemyActor enemy = EnemyPool.GetEnemy(spawner.transform);

            enemy.ParentSpawner = spawner;
            Vector3 positionOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            //Vector3 positionOffset = new Vector3(-0.5f, -0.5f, 0);
            enemy.positionOffset = positionOffset;
            enemy.rotatedOffset = positionOffset;

            enemy.SetBase(enemyBase, rarity, stageInfo.monsterLevel);
            enemy.Data.SetMobBonuses(bonuses);
            if (enemyBase.isBoss || enemyWaveItem.isBossOverride)
                enemy.isBoss = true;

            enemy.Init();

            currentEnemyList.Add(enemy);
            enemiesSpawned++;
            yield return new WaitForSeconds(delayBetween);
        }
        spawnCoroutinesRunning--;
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