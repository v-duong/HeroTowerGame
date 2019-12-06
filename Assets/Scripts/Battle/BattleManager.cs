using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private const int BASE_UNCOMMON_DROP_WEIGHT = 140;
    private const int BASE_RARE_DROP_WEIGHT = 50;
    private const int BASE_EPIC_DROP_WEIGHT = 6;
    private const int BASE_UNIQUE_DROP_WEIGHT = 4;

    private const int BASE_UNCOMMON_DROP_WEIGHT_STAGE_DROP = 20;
    private const int BASE_RARE_DROP_WEIGHT_STAGE_DROP = 75;
    private const int BASE_EPIC_DROP_WEIGHT_STAGE_DROP = 5;

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
    private int gainedExpThisLoop = 0;
    private int gainedFragments = 0;
    private int gainedFragmentsThisLoop = 0;
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
            if (survivalLoopCount == 0)
                GameManager.Instance.PlayerStats.AddToStageClearCount(stageInfo.idName);

            battleEndWindow.ShowVictoryWindow();

            CalculateExpGain(battleEndWindow);
            CalculateItemFragmentDrops(battleEndWindow);

            //AddConsumableDrops(battleEndWindow);
            AddEquipmentDrops(battleEndWindow);
            AddArchetypeDrops(battleEndWindow);
        }
        else
        {
            battleEndWindow.ShowLoseWindow();

            gainedExpThisLoop = gainedExp / -2;
            gainedExp /= 2;
            gainedFragmentsThisLoop = gainedFragments / -2;
            gainedFragments /= 2;

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

        battleEndWindow.SetExpGainValues(gainedExp, gainedExpThisLoop);
        battleEndWindow.SetFragGainValues(gainedFragments, gainedFragmentsThisLoop);
        battleEndWindow.UpdateExpString();
        battleEndWindow.UpdateFragString();

        if (gainedArchetypeItems.Count > 0)
        {
            string gainedArchetypesString = "Archetypes:\n";
            Dictionary<string, int> archetypeCount = new Dictionary<string, int>();
            foreach (ArchetypeItem archetypeItem in gainedArchetypeItems)
            {
                if (!archetypeCount.ContainsKey(archetypeItem.Name))
                    archetypeCount.Add(archetypeItem.Name, 0);
                archetypeCount[archetypeItem.Name]++;
            }

            foreach (KeyValuePair<string, int> archetypeEntry in archetypeCount)
            {
                gainedArchetypesString += "<indent=10%>" + archetypeEntry.Key + " x" + archetypeEntry.Value + "</indent>\n";
            }

            gainedArchetypesString += '\n';

            battleEndWindow.AddToBodyText(gainedArchetypesString);
        }

        if (gainedEquipment.Count > 0)
        {
            string gainEquipString = "Equipment:\n";
            Dictionary<string, int> equipmentCount = new Dictionary<string, int>();
            foreach (Equipment equipment in gainedEquipment)
            {
                if (!equipmentCount.ContainsKey(equipment.Base.idName))
                    equipmentCount.Add(equipment.Base.idName, 0);
                equipmentCount[equipment.Base.idName]++;
            }

            foreach (KeyValuePair<string, int> equipEntry in equipmentCount)
            {
                gainEquipString += "<indent=10%>" + LocalizationManager.Instance.GetLocalizationText_Equipment(equipEntry.Key) + " x" + equipEntry.Value + "</indent>\n";
            }

            battleEndWindow.AddToBodyText(gainEquipString);
        }
    }

    public void AllocateRewards()
    {
        foreach (HeroData hero in GameManager.Instance.inBattleHeroes)
        {
            hero.AddExperience(gainedExp);
        }

        PlayerStats playerStats = GameManager.Instance.PlayerStats;

        playerStats.ModifyExpStock((int)(gainedExp * PlayerStats.EXP_STOCK_RATE));
        playerStats.ModifyItemFragments(gainedFragments);

        foreach (Equipment equip in gainedEquipment)
        {
            playerStats.AddEquipmentToInventory(equip);
        }

        foreach (ArchetypeItem archetypeItem in gainedArchetypeItems)
        {
            playerStats.AddArchetypeToInventory(archetypeItem);
        }
    }

    private void AddArchetypeDrops(BattleEndWindow battleEndWindow)
    {
        //Get Archetype
        int archetypeDrops = 1 + survivalLoopCount / 3;
        int i = 0;
        if (stageInfo.archetypeDropList.Count != 0)
        {
            WeightList<string> stageDropList = Helpers.CreateWeightListFromWeightBases(stageInfo.archetypeDropList);
            string baseId = stageDropList.ReturnWeightedRandom();
            ArchetypeBase archetypeBase = ResourceManager.Instance.GetArchetypeBase(baseId);
            ArchetypeItem item = ArchetypeItem.CreateArchetypeItem(archetypeBase, stageLevel);
            gainedArchetypeItems.Add(item);
            i = 1;
            
        } 

        for (; i < archetypeDrops; i++)
        {
            ArchetypeItem item = ArchetypeItem.CreateArchetypeItem(ResourceManager.Instance.GetRandomArchetypeBase(stageLevel + survivalLoopCount), stageLevel + survivalLoopCount);
            gainedArchetypeItems.Add(item);
        }
    }

    private void AddEquipmentDrops(BattleEndWindow battleEndWindow)
    {
        //Get Equipment
        int additionalDrops = (int)(survivalLoopCount / 4);
        float rarityBoost = 1 + (0.25f * survivalLoopCount);
        float stageEpicBoost = 1 + (0.25f * survivalLoopCount * 4);
        float affixLevelSkew = 1.1f + (survivalLoopCount * 0.15f);
        int equipmentDrops = Random.Range(stageInfo.equipmentDropCountMin + additionalDrops, stageInfo.equipmentDropCountMax + 1 + additionalDrops);

        WeightList<RarityType> nonStageDropRarity = new WeightList<RarityType>();
        nonStageDropRarity.Add(RarityType.UNCOMMON, (int)(BASE_UNCOMMON_DROP_WEIGHT / rarityBoost));
        nonStageDropRarity.Add(RarityType.RARE, (int)(BASE_RARE_DROP_WEIGHT * rarityBoost));
        nonStageDropRarity.Add(RarityType.EPIC, (int)(BASE_EPIC_DROP_WEIGHT * rarityBoost));
        nonStageDropRarity.Add(RarityType.UNIQUE, (int)(BASE_UNIQUE_DROP_WEIGHT * rarityBoost));

        WeightList<RarityType> stageDropRarity = new WeightList<RarityType>();
        stageDropRarity.Add(RarityType.UNCOMMON, (int)(BASE_UNCOMMON_DROP_WEIGHT_STAGE_DROP / rarityBoost));
        stageDropRarity.Add(RarityType.RARE, (int)(BASE_RARE_DROP_WEIGHT_STAGE_DROP * rarityBoost));
        stageDropRarity.Add(RarityType.EPIC, (int)(BASE_EPIC_DROP_WEIGHT_STAGE_DROP * stageEpicBoost));

        if (stageInfo.equipmentDropList.Count == 0)
        {
            int boostedRarityDrops = System.Math.Max(equipmentDrops / 2, 1);
            for (int i = 0; i < equipmentDrops; i++)
            {
                RarityType rarity;
                if (i < boostedRarityDrops)
                    rarity = stageDropRarity.ReturnWeightedRandom();
                else
                    rarity = nonStageDropRarity.ReturnWeightedRandom();
                AddNonStagePoolDrop(affixLevelSkew, rarity);
            }
        }
        else
        {
            WeightList<string> weightList = Helpers.CreateWeightListFromWeightBases(stageInfo.equipmentDropList);
            int dropsFromStagePool = System.Math.Max(equipmentDrops / 2, 1);
            for (int i = 0; i < dropsFromStagePool; i++)
            {
                string baseId = weightList.ReturnWeightedRandom();
                var equip = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase(baseId), stageLevel + survivalLoopCount);
                RollEquipmentRarity(equip, stageDropRarity.ReturnWeightedRandom(), affixLevelSkew);
                gainedEquipment.Add(equip);
            }

            for (int i = 0; i < equipmentDrops - dropsFromStagePool; i++)
            {
                RarityType rarity = nonStageDropRarity.ReturnWeightedRandom();
                AddNonStagePoolDrop(affixLevelSkew, rarity);
            }
        }
    }

    private void AddNonStagePoolDrop(float affixLevelSkew, RarityType rarity)
    {
        Equipment equip;
        if (rarity != RarityType.UNIQUE)
        {
            equip = Equipment.CreateRandomEquipment(stageLevel + survivalLoopCount);
            RollEquipmentRarity(equip, rarity, affixLevelSkew);
        }
        else
        {
            equip = Equipment.CreateRandomUnique(stageLevel + survivalLoopCount);
            if (equip == null)
            {
                equip = Equipment.CreateRandomEquipment(stageLevel + survivalLoopCount);
                RollEquipmentRarity(equip, RarityType.EPIC, affixLevelSkew);
            }
        }
        gainedEquipment.Add(equip);
    }

    private void RollEquipmentRarity(Equipment equip, RarityType rarity, float affixLevelSkew)
    {
        equip.SetRarity(rarity);
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

    private void CalculateItemFragmentDrops(BattleEndWindow battleEndWindow)
    {
        double multiplier = System.Math.Pow(1.18f, survivalLoopCount);
        int minDrop = (int)(stageInfo.consumableDropCountMin * multiplier);
        int maxDrop = (int)(stageInfo.consumableDropCountMax * multiplier);
        gainedFragmentsThisLoop = Random.Range(minDrop, maxDrop + 1);
        gainedFragments += gainedFragmentsThisLoop;
    }

    private void CalculateExpGain(BattleEndWindow battleEndWindow)
    {
        double multiplier = System.Math.Pow(1.15f, survivalLoopCount) - 1;
        gainedExpThisLoop = (int)(stageInfo.baseExperience * (stageInfo.expMultiplier + multiplier));
        gainedExp += gainedExpThisLoop;
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