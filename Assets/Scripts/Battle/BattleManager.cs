using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private const float BASE_RARE_DROP_RATE = 0.2f;
    private const float REQUIRED_WAIT_TIME = 0.2f;
    public BattlePlayerInfoPanel battleInfo;

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
    public List<HeroActor> activeHeroes = new List<HeroActor>();

    public int enemiesSpawned;
    public int selectedTeam;
    public int playerHealth = 20;
    public float playerSoulpoints = 10;
    public int stageLevel = 1;
    public int currentWave;

    private int gainedExp = 0;
    private List<Equipment> gainedEquipment = new List<Equipment>();
    private List<ArchetypeItem> gainedArchetypeItems = new List<ArchetypeItem>();

    private float waveTimeElapsed;
    private float currentWaveDelay;

    public bool startedSpawn = false;
    public bool finishedSpawn = false;
    public bool battleEnded = false;

    public bool isSurvivalBattle = false;
    public int survivalLoopCount = 0;

    // Use this for initialization
    private void Start()
    {
        currentWave = 0;
        enemiesSpawned = 0;
        playerHealth = 30;
    }

    private void Update()
    {
        if (!battleEnded)
        {
            waveTimeElapsed += Time.deltaTime;
            if (waveTimeElapsed < currentWaveDelay * REQUIRED_WAIT_TIME)
                battleInfo.nextWavePanel.startwaveButton.interactable = false;
            else
                battleInfo.nextWavePanel.startwaveButton.interactable = true;

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

    public void Initialize()
    {
        battleInfo.InitializeNextWaveInfo(Waves[0].enemyList, null, 0, 1, true);
    }

    public void ModifyPlayerHealth(int value, bool causedByGoal)
    {
        if (value < 0 && causedByGoal)
        {
            playerHealth += value * (survivalLoopCount + 1);
        }
        else
            playerHealth += value;
    }

    public void StartBattle()
    {
        if (!startedSpawn && Waves != null)
        {
            startedSpawn = true;
            SpawnWave(0);
        }
    }

    public void StartSurvivalBattle()
    {
        isSurvivalBattle = true;
        battleEnded = false;
        finishedSpawn = false;
        survivalLoopCount++;
        GameManager.Instance.isInBattle = true;
        SpawnWave(0);
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

    public void EndBattle(bool victory)
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
        battleEndWindow.isVictory = victory;

        if (victory)
        {
            Debug.Log("VIC");
            if (survivalLoopCount == 0)
                GameManager.Instance.PlayerStats.AddToStageClearCount(stageInfo.idName);

            battleEndWindow.ShowVictoryWindow();

            CalculateExpGain(battleEndWindow);

            //AddConsumableDrops(battleEndWindow);
            AddEquipmentDrops(battleEndWindow);
            AddArchetypeDrops(battleEndWindow);
        }
        else
        {
            Debug.Log("FAIL");
            battleEndWindow.ShowLoseWindow();

            gainedExp /= 2;

            if (gainedArchetypeItems.Count > 0)
            {
                int numToRemove = System.Math.Max(gainedArchetypeItems.Count / 2, 1);
                for (int i = 0; i < numToRemove; i++)
                {
                    gainedArchetypeItems.RemoveAt(Random.Range(0, gainedArchetypeItems.Count));
                }
            }

            if (gainedEquipment.Count > 0)
            {
                int numToRemove = System.Math.Max(gainedEquipment.Count / 2, 1);
                for (int i = 0; i < numToRemove; i++)
                {
                    gainedEquipment.RemoveAt(Random.Range(0, gainedEquipment.Count));
                }
            }
        }

        battleEndWindow.AddToBodyText("Experience: " + gainedExp + "\nStocked Experience: " + (gainedExp * 0.25f) + "\n");

        if (gainedArchetypeItems.Count > 0)
        {
            string gainedArchetypesString = "Archetypes: ";
            foreach (ArchetypeItem archetypeItem in gainedArchetypeItems)
            {
                gainedArchetypesString += archetypeItem.Name + ", ";
            }
            gainedArchetypesString = gainedArchetypesString.Trim(',', ' ') + "\n";
            battleEndWindow.AddToBodyText(gainedArchetypesString);
        }
    }

    public void AllocateRewards(bool isVictory)
    {
        foreach (HeroData hero in GameManager.Instance.inBattleHeroes)
        {
            hero.AddExperience(gainedExp);
        }
        GameManager.Instance.PlayerStats.ModifyExpStock((int)(gainedExp * 0.25f));

        foreach (Equipment equip in gainedEquipment)
        {
            GameManager.Instance.PlayerStats.AddEquipmentToInventory(equip);
        }

        foreach (ArchetypeItem archetypeItem in gainedArchetypeItems)
        {
            GameManager.Instance.PlayerStats.AddArchetypeToInventory(archetypeItem);
        }
    }

    private void AddArchetypeDrops(BattleEndWindow battleEndWindow)
    {
        //Get Archetype
        if (stageInfo.archetypeDropList.Count != 0)
        {
            int archetypeDrops = 1 + survivalLoopCount / 2;
            for (int i = 0; i < archetypeDrops; i++)
            {
                WeightList<string> weightList = Helpers.CreateWeightListFromWeightBases(stageInfo.archetypeDropList);
                string baseId = weightList.ReturnWeightedRandom();
                ArchetypeBase archetypeBase = ResourceManager.Instance.GetArchetypeBase(baseId);
                ArchetypeItem item = ArchetypeItem.CreateArchetypeItem(archetypeBase, stageLevel);
                gainedArchetypeItems.Add(item);
            }
        }
    }

    private void AddEquipmentDrops(BattleEndWindow battleEndWindow)
    {
        //Get Equipment
        int additionalDrops = (int)(survivalLoopCount * 0.25f);
        int equipmentDrops = Random.Range(stageInfo.equipmentDropCountMin + additionalDrops, stageInfo.equipmentDropCountMax + 1 + additionalDrops);
        if (stageInfo.equipmentDropList.Count == 0)
        {
            for (int i = 0; i < equipmentDrops; i++)
            {
                var equip = Equipment.CreateRandomEquipment(stageLevel + survivalLoopCount);
                RollEquipmentRarity(equip);
                gainedEquipment.Add(equip);
            }
        }
        else
        {
            WeightList<string> weightList = Helpers.CreateWeightListFromWeightBases(stageInfo.equipmentDropList);
            int dropsFromStagePool = System.Math.Max(equipmentDrops / 2, 1);
            for (int i = 0; i < dropsFromStagePool; i++)
            {
                string baseId = weightList.ReturnWeightedRandom();
                EquipmentBase equipmentBase = ResourceManager.Instance.GetEquipmentBase(baseId);
                var equip = Equipment.CreateEquipmentFromBase(equipmentBase, stageLevel + survivalLoopCount);
                RollEquipmentRarity(equip);
                gainedEquipment.Add(equip);
            }

            for (int i = 0; i < equipmentDrops - dropsFromStagePool; i++)
            {
                var equip = Equipment.CreateRandomEquipment(stageLevel + survivalLoopCount);
                RollEquipmentRarity(equip);
                gainedEquipment.Add(equip);
            }
        }
    }

    private void RollEquipmentRarity(Equipment equip)
    {
        float rareChance = BASE_RARE_DROP_RATE * (survivalLoopCount + 1);
        float affixLevelSkew = 1.1f + (survivalLoopCount * 0.1f);
        if (rareChance > 1f)
        {
            float epicChance = (rareChance - 1f) / 4f;
            equip.SetRarity(Random.Range(0f, 1f) < epicChance ? RarityType.EPIC : RarityType.RARE);
        }
        else
        {
            equip.SetRarity(Random.Range(0f, 1f) < rareChance ? RarityType.RARE : RarityType.UNCOMMON);
        }

        equip.RerollAffixesAtRarity(null, affixLevelSkew, new HashSet<GroupType>() { GroupType.DROP_ONLY });
    }

    private void AddConsumableDrops(BattleEndWindow battleEndWindow)
    {
        // Get Consumables
        int consumableDrops = Random.Range(stageInfo.consumableDropCountMin, stageInfo.consumableDropCountMax + 1);
        Dictionary<ConsumableType, int> consumablesCount = new Dictionary<ConsumableType, int>();
        for (int i = 0; i < consumableDrops; i++)
        {
            ConsumableType consumable = GameManager.Instance.GetRandomConsumable();
            GameManager.Instance.PlayerStats.consumables[consumable]++;
            if (!consumablesCount.ContainsKey(consumable))
                consumablesCount.Add(consumable, 0);
            consumablesCount[consumable]++;
        }

        foreach (KeyValuePair<ConsumableType, int> keyValue in consumablesCount)
        {
            battleEndWindow.AddToBodyText(keyValue.Key.ToString() + " +" + keyValue.Value + "\n");
        }
    }

    private void CalculateExpGain(BattleEndWindow battleEndWindow)
    {
        int experience = (int)(stageInfo.baseExperience * (stageInfo.expMultiplier + (0.15f * survivalLoopCount)));

        gainedExp += experience;
    }

    public void SpawnWave(int wave)
    {
        currentWave = wave;
        waveCoroutine = StartCoroutine(SpawnWaveCo());
        //currentWave++;
    }

    public void RushNextWave()
    {
        if (waveTimeElapsed < currentWaveDelay * REQUIRED_WAIT_TIME)
            return;

        StopCoroutine(waveCoroutine);
        currentWave++;
        waveCoroutine = StartCoroutine(SpawnWaveCo());
    }

    private IEnumerator SpawnWaveCo()
    {
        int waveNumber = currentWave;
        EnemyWave waveToSpawn = Waves[waveNumber];

        for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
        {
            StartCoroutine(SpawnEnemyList(waveToSpawn.enemyList[i], waveToSpawn.delayBetweenSpawns));
        }

        if (waveNumber + 1 < Waves.Count)
        {
            /*
            while(timeUntilNextWave > 0)
            {
                timeUntilNextWave -= Time.deltaTime;
                battleInfo.UpdateNextWaveInfo(Waves[waveNumber + 1].enemyList, timeUntilNextWave);
                yield return null;
            }
            */
            List<EnemyWaveItem> waveAfter = null;
            if (waveNumber + 2 < Waves.Count)
                waveAfter = Waves[waveNumber + 2].enemyList;

            battleInfo.InitializeNextWaveInfo(Waves[waveNumber + 1].enemyList, waveAfter, waveToSpawn.delayUntilNextWave, currentWave + 2);

            waveTimeElapsed = 0;
            currentWaveDelay = waveToSpawn.delayUntilNextWave;

            yield return new WaitForSeconds(waveToSpawn.delayUntilNextWave);
            currentWave++;
            waveCoroutine = StartCoroutine(SpawnWaveCo());
        }
        else
        {
            battleInfo.InitializeNextWaveInfo(null, null, 0, currentWave + 2);
            battleInfo.soulPointText.text = "";
            finishedSpawn = true;
        }

        yield break;
    }

    private IEnumerator SpawnEnemyList(EnemyWaveItem enemyWaveItem, float delayBetween)
    {
        spawnCoroutinesRunning++;

        yield return new WaitForSeconds(enemyWaveItem.startDelay);

        Dictionary<BonusType, StatBonus> bonuses = new Dictionary<BonusType, StatBonus>();
        EnemyBase enemyBase = ResourceManager.Instance.GetEnemyBase(enemyWaveItem.enemyName);
        RarityType rarity = enemyWaveItem.enemyRarity;

        if (isSurvivalBattle)
        {
            int totalWaveCount = survivalLoopCount * Waves.Count + (currentWave + 1);

            StatBonus healthBonus = new StatBonus();
            healthBonus.AddBonus(ModifyType.MULTIPLY, 25 * survivalLoopCount);
            bonuses.Add(BonusType.MAX_HEALTH, healthBonus);

            StatBonus damageBonus = new StatBonus();
            damageBonus.AddBonus(ModifyType.MULTIPLY, 15 * survivalLoopCount);
            bonuses.Add(BonusType.GLOBAL_DAMAGE, damageBonus);
        }

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

            if (enemyBase.isBoss || enemyWaveItem.isBossOverride)
                enemy.isBoss = true;
            else
                enemy.isBoss = false;

            enemy.SetBase(enemyBase, rarity, stageInfo.monsterLevel + survivalLoopCount);

            //Set bonuses from wave
            enemy.Data.SetMobBonuses(bonuses);

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