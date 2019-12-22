﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerStats PlayerStats { get; private set; }

    [SerializeField]
    public Projectile projectilePrefab;

    [SerializeField]
    public Projectile boxProjectilePrefab;

    public float aspectRatio;
    public int selectedTeamNum;
    public bool isInBattle;
    public bool isInMainMenu;
    public List<HeroData> inBattleHeroes = new List<HeroData>();

    private string currentSceneName = "";

    private WeightList<ConsumableType> consumableWeightList;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        aspectRatio = (float)Screen.height / (float)Screen.width;
        QualitySettings.vSyncCount = 1;

        currentSceneName = "mainMenu";

#if !UNITY_EDITOR
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
#endif
        isInBattle = false;
        isInMainMenu = true;

        /*
        for (int i = 0; i < 50; i++)
        {
            Equipment equipment = Equipment.CreateRandomEquipment(100);
            equipment.SetRarity((RarityType)Random.Range(2, 4));
            equipment.RerollAffixesAtRarity();
            PlayerStats.AddEquipmentToInventory(equipment);
        }
        */

#if UNITY_EDITOR
        CheckForBonuses();
#endif

        PlayerStats = new PlayerStats();
        if (!SaveManager.Load())
        {
            AddStartingData();
            SaveManager.SaveAll();
        }
    }

    public void InitializePlayerStats()
    {
        PlayerStats = new PlayerStats();
        AddStartingData();
        SaveManager.SaveAll();
        StartCoroutine(ReloadMainMenu());
    }

    private IEnumerator ReloadMainMenu()
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync("mainMenu");
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
    }

    private void AddStartingData()
    {
        Equipment startingSword = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase("OneHandedSword1"), 1);
        Equipment startingBow = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase("Bow1"), 1);
        Equipment startingWand = Equipment.CreateEquipmentFromBase(ResourceManager.Instance.GetEquipmentBase("Wand1"), 1);

        startingSword.SetRarity(RarityType.UNCOMMON);
        startingSword.AddAffix(ResourceManager.Instance.GetAffixBase("LocalPhysicalDamageAdditive1", AffixType.PREFIX));
        startingSword.AddAffix(ResourceManager.Instance.GetAffixBase("StrFlat1", AffixType.SUFFIX));

        startingBow.SetRarity(RarityType.UNCOMMON);
        startingBow.AddAffix(ResourceManager.Instance.GetAffixBase("LocalPhysicalDamageAdditive1", AffixType.PREFIX));
        startingBow.AddAffix(ResourceManager.Instance.GetAffixBase("AgiFlat1", AffixType.SUFFIX));

        startingWand.SetRarity(RarityType.UNCOMMON);
        startingWand.AddAffix(ResourceManager.Instance.GetAffixBase("SpellDamage1", AffixType.PREFIX));
        startingWand.AddAffix(ResourceManager.Instance.GetAffixBase("IntFlat1", AffixType.SUFFIX));

        PlayerStats.AddEquipmentToInventory(startingSword);
        PlayerStats.AddEquipmentToInventory(startingBow);
        PlayerStats.AddEquipmentToInventory(startingWand);

        HeroData startingSoldier = HeroData.CreateNewHero("Soldier", ResourceManager.Instance.GetArchetypeBase("Soldier"), ResourceManager.Instance.GetArchetypeBase("Novice"));
        HeroData startingRanger = HeroData.CreateNewHero("Ranger", ResourceManager.Instance.GetArchetypeBase("Ranger"), ResourceManager.Instance.GetArchetypeBase("Novice"));
        HeroData startingMage = HeroData.CreateNewHero("Mage", ResourceManager.Instance.GetArchetypeBase("Mage"), ResourceManager.Instance.GetArchetypeBase("Novice"));

        PlayerStats.AddHeroToList(startingSoldier);
        PlayerStats.AddHeroToList(startingRanger);
        PlayerStats.AddHeroToList(startingMage);

        PlayerStats.SetHeroToTeamSlot(startingSoldier, 0, 0);
        PlayerStats.SetHeroToTeamSlot(startingRanger, 0, 1);
        PlayerStats.SetHeroToTeamSlot(startingMage, 0, 2);
    }

    private void CheckForBonuses()
    {
        foreach (ArchetypeBase archetypeBase in ResourceManager.Instance.ArchetypeBasesList)
        {
            /*
            PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateArchetypeItem(archetypeBase, 100));
            foreach (var ability in archetypeBase.GetArchetypeAbilities(false))
            {
                PlayerStats.AddAbilityToInventory(AbilityCoreItem.CreateAbilityItemFromArchetype(archetypeBase, ability));
            }
            */
            foreach (var x in archetypeBase.nodeList)
            {
                foreach (var y in x.bonuses)
                {
                    LocalizationManager.Instance.GetBonusTypeString(y.bonusType);
                }
            }
        }
    }

    public ConsumableType GetRandomConsumable()
    {
        if (consumableWeightList == null)
        {
            consumableWeightList = new WeightList<ConsumableType>();
            consumableWeightList.Add(ConsumableType.AFFIX_REROLLER, 3000);
            consumableWeightList.Add(ConsumableType.AFFIX_CRAFTER, 900);
        }
        return consumableWeightList.ReturnWeightedRandom();
    }

    public void AddRandomConsumableToInventory()
    {
        PlayerStats.consumables[GetRandomConsumable()] += 1;
    }

    public void MoveToMainMenu()
    {
        isInBattle = false;

        SaveManager.SaveAll();

        SceneManager.UnloadSceneAsync(currentSceneName);
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
        Resources.UnloadUnusedAssets();

        isInMainMenu = true;
    }

    /// <summary>
    /// Loads loading scene and starts coroutine for the battle/stage loading.
    /// </summary>
    /// <param name="stageInfoBase"></param>
    public void MoveToBattle(StageInfoBase stageInfoBase)
    {
        SetTimescale(1);
        SaveManager.SaveAll();
        SceneManager.LoadScene("loadingScene", LoadSceneMode.Additive);
        currentSceneName = "stage" + stageInfoBase.sceneAct + '-' + stageInfoBase.sceneStage;

        StartCoroutine(LoadBattleRoutine(currentSceneName, stageInfoBase));

        Camera.main.transform.position = new Vector3(0, 0, -10);
        SceneManager.UnloadSceneAsync("mainMenu");

        isInMainMenu = false;
    }

    /// <summary>
    ///  Loads battle/stage scene and sets active scene to it.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="stageInfoBase"></param>
    /// <returns></returns>
    private IEnumerator LoadBattleRoutine(string sceneName, StageInfoBase stageInfoBase)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("battleUI", LoadSceneMode.Additive);
        AsyncOperation asyncOperation2 = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncOperation.isDone || !asyncOperation2.isDone)
        {
            yield return null;
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);

        SetUpBattleScene(scene, stageInfoBase);

        StageManager.Instance.BattleManager.selectedTeam = selectedTeamNum;
        StageManager.Instance.BattleManager.SetStageBase(stageInfoBase);
        StageManager.Instance.BattleManager.InitializeProjectilePool();
        StageManager.Instance.InitalizeStage();
        InputManager.Instance.ResetManager();
        ParticleManager.Instance.ClearParticleSystems();
        ParticleManager.Instance.InitializeHitEffectInstances();
        isInBattle = true;

        yield return null;

        UIManager.Instance.LoadingScreen.endLoadingScreen = true;
    }

    /// <summary>
    /// Coroutine for loading the UI for battle scene. Instantiates actors for the selected hero team.
    /// </summary>
    /// <param name="sceneToMergeTo"></param>
    /// <returns></returns>
    private void SetUpBattleScene(Scene sceneToMergeTo, StageInfoBase stageInfoBase)
    {
        Scene scene = SceneManager.GetSceneByName("battleUI");
        SceneManager.MergeScenes(scene, sceneToMergeTo);
        SummonScrollWindow summonScroll = UIManager.Instance.SummonScrollWindow;

        HashSet<AbilityBase> abilitiesInUse = new HashSet<AbilityBase>();
        HashSet<EnemyBase> enemiesInUse = new HashSet<EnemyBase>();
        inBattleHeroes.Clear();

        foreach (HeroData data in PlayerStats.heroTeams[selectedTeamNum])
        {
            if (data == null)
                continue;

            data.ClearTemporaryBonuses(true);

            GameObject actor = Instantiate(ResourceManager.Instance.HeroPrefab.gameObject);
            data.InitHeroActor(actor);
            HeroActor heroActor = actor.GetComponent<HeroActor>();

            if (heroActor == null)
                continue;

            foreach (AbilityBase abilityBase in heroActor.GetAbilitiyBasesInList())
            {
                abilitiesInUse.Add(abilityBase);
                if (abilityBase.hasLinkedAbility)
                {
                    AbilityBase linkedBase = ResourceManager.Instance.GetAbilityBase(abilityBase.linkedAbility.abilityId);
                    while (linkedBase != null)
                    {
                        abilitiesInUse.Add(linkedBase);

                        if (linkedBase.hasLinkedAbility)
                            linkedBase = ResourceManager.Instance.GetAbilityBase(linkedBase.linkedAbility.abilityId);
                        else
                            linkedBase = null;
                    }
                }
            }

            summonScroll.AddHeroActor(heroActor);
            inBattleHeroes.Add(data);
        }

        foreach (EnemyWave enemyWave in stageInfoBase.enemyWaves)
        {
            foreach (EnemyWaveItem enemyWaveItem in enemyWave.enemyList)
            {
                EnemyBase enemyBase = ResourceManager.Instance.GetEnemyBase(enemyWaveItem.enemyName);
                enemiesInUse.Add(enemyBase);
                foreach (EnemyBase.EnemyAbilityBase ability in enemyBase.abilitiesList)
                {
                    abilitiesInUse.Add(ResourceManager.Instance.GetAbilityBase(ability.abilityName));
                }
            }
        }

        /*
        foreach (AbilityBase abilityBase in abilitiesInUse)
            Debug.Log(abilityBase.idName);
            */

        ResourceManager.Instance.LoadAbilitySpritesToBeUsed(abilitiesInUse);
        ResourceManager.Instance.LoadEnemySpritesToBeUsed(enemiesInUse);
    }

    public static void SetTimescale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}