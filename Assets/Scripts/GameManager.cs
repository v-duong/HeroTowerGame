using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private Coroutine currentCoroutine;

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

        StartCoroutine(StartRoutine());

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
    }

    private IEnumerator StartRoutine()
    {
        PlayerStats = new PlayerStats();
        if (!SaveManager.Load())
        {
            AddStartingData();
            SaveManager.SaveAll();
        }

#if !UNITY_EDITOR
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("mainMenu", LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
#endif

        if (!PlayerStats.hasSeenStartingMessage)
            OpenTutorialMessage();

        yield break;
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
        yield return null;
        if (!PlayerStats.hasSeenStartingMessage)
            OpenTutorialMessage();
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

        startingSoldier.spriteName = "hero2";
        startingRanger.spriteName = "hero1";
        startingMage.spriteName = "hero4";

        PlayerStats.AddHeroToList(startingSoldier);
        PlayerStats.AddHeroToList(startingRanger);
        PlayerStats.AddHeroToList(startingMage);

        PlayerStats.SetHeroToTeamSlot(startingSoldier, 0, 0);
        PlayerStats.SetHeroToTeamSlot(startingRanger, 0, 1);
        PlayerStats.SetHeroToTeamSlot(startingMage, 0, 2);
        PlayerStats.hasSeenStartingMessage = false;
    }

    private void OpenTutorialMessage()
    {
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenTextWindow("");
        popUpWindow.SetButtonValues(null, null, "Close", delegate
        {
            UIManager.Instance.CloseCurrentWindow();
            PlayerStats.hasSeenStartingMessage = true;
            SaveManager.CurrentSave.SavePlayerData();
            SaveManager.Save();
        });
        popUpWindow.textField.text = "You should start by assigning each hero their starting ability and weapons. You'll find them by tapping the Heroes button.\n\nIn the corner of some windows, there is a question mark you can tap to bring up a help page.\n\nThis game is still under development and only up to World 3 has been added.";
        popUpWindow.textField.fontSize = 18;
        popUpWindow.textField.paragraphSpacing = 8;
        popUpWindow.textField.alignment = TextAlignmentOptions.Left;
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
                    if (y.restriction != GroupType.NO_GROUP)
                        LocalizationManager.Instance.GetLocalizationText_GroupTypeRestriction(y.restriction);
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

    public IEnumerator MoveToMainMenu()
    {
        isInBattle = false;
        SceneManager.LoadScene("loadingScene", LoadSceneMode.Additive);

        SaveManager.SaveAll();
        AsyncOperation asyncOperation1 = SceneManager.UnloadSceneAsync(currentSceneName);
        AsyncOperation asyncOperation2 = SceneManager.LoadSceneAsync("mainMenu", LoadSceneMode.Additive);

        while (!asyncOperation1.isDone || !asyncOperation2.isDone)
        {
            yield return null;
        }
        UIManager.Instance.LoadingScreen.endLoadingScreen = true;
        Resources.UnloadUnusedAssets();

        isInMainMenu = true;
    }

    /// <summary>
    /// Loads loading scene and starts coroutine for the battle/stage loading.
    /// </summary>
    /// <param name="stageInfoBase"></param>
    public void MoveToBattle(StageInfoBase stageInfoBase)
    {
        UIManager.Instance.CloseAllWindows();
        SetTimescale(1);

        SaveManager.SaveAll();
        currentSceneName = "stage" + stageInfoBase.sceneAct + '-' + stageInfoBase.sceneStage;

        if (currentCoroutine == null)
            currentCoroutine = StartCoroutine(LoadBattleRoutine(currentSceneName, stageInfoBase));
        else
        {
            return;
        }

        Camera.main.transform.position = new Vector3(0, 0, -10);

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
        SceneManager.LoadScene("loadingScene", LoadSceneMode.Additive);
        yield return null;
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
        SceneManager.UnloadSceneAsync("mainMenu");
        yield return null;
        currentCoroutine = null;

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