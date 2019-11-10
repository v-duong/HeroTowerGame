using System.Collections;
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

    public int selectedTeamNum;
    public bool isInBattle;
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
        QualitySettings.vSyncCount = 1;
        isInBattle = false;
        currentSceneName = "mainMenu";

#if !UNITY_EDITOR
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
#endif

        PlayerStats = new PlayerStats();
        /*
        for (int i = 0; i < 50; i++)
        {
            Equipment equipment = Equipment.CreateRandomEquipment(100);
            equipment.SetRarity((RarityType)Random.Range(2, 4));
            equipment.RerollAffixesAtRarity();
            PlayerStats.AddEquipmentToInventory(equipment);
        }

        foreach (ArchetypeBase archetypeBase in ResourceManager.Instance.ArchetypeBasesList)
        {
            PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateArchetypeItem(archetypeBase, 100));
            foreach (var ability in archetypeBase.GetArchetypeAbilities(false))
            {
                PlayerStats.AddAbilityToInventory(AbilityCoreItem.CreateAbilityItemFromArchetype(archetypeBase, ability));
            }
        }
        */

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

        HeroData startingSoldier = HeroData.CreateNewHero("Soldier", ArchetypeItem.CreateArchetypeItem(ResourceManager.Instance.GetArchetypeBase("Soldier"), 1));
        HeroData startingRanger = HeroData.CreateNewHero("Ranger", ArchetypeItem.CreateArchetypeItem(ResourceManager.Instance.GetArchetypeBase("Ranger"), 1));
        HeroData startingMage = HeroData.CreateNewHero("Mage", ArchetypeItem.CreateArchetypeItem(ResourceManager.Instance.GetArchetypeBase("Mage"), 1));

        PlayerStats.AddHeroToList(startingSoldier);
        PlayerStats.AddHeroToList(startingRanger);
        PlayerStats.AddHeroToList(startingMage);
    }

    public ConsumableType GetRandomConsumable()
    {
        if (consumableWeightList == null)
        {
            consumableWeightList = new WeightList<ConsumableType>();
            consumableWeightList.Add(ConsumableType.LOW_TIER_UPGRADER, 2000);
            consumableWeightList.Add(ConsumableType.RARE_TO_EPIC, 30);
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

        SceneManager.UnloadSceneAsync(currentSceneName);
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Loads loading scene and starts coroutine for the battle/stage loading.
    /// </summary>
    /// <param name="stageInfoBase"></param>
    public void MoveToBattle(StageInfoBase stageInfoBase)
    {
        SetTimescale(1);
        SceneManager.LoadScene("loadingScene", LoadSceneMode.Additive);
        currentSceneName = "stage" + stageInfoBase.sceneAct + '-' + stageInfoBase.sceneStage;

        StartCoroutine(LoadBattleRoutine(currentSceneName, stageInfoBase));

        Camera.main.transform.position = new Vector3(0, 0, -10);
        SceneManager.UnloadSceneAsync("mainMenu");
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

        SetUpBattleScene(scene);

        StageManager.Instance.BattleManager.selectedTeam = selectedTeamNum;
        StageManager.Instance.BattleManager.SetStageBase(stageInfoBase);
        StageManager.Instance.BattleManager.InitializeProjectilePool();
        StageManager.Instance.InitalizeStage();
        InputManager.Instance.ResetManager();
        ParticleManager.Instance.ClearParticleSystems();
        isInBattle = true;

        UIManager.Instance.LoadingScreen.endLoadingScreen = true;
    }

    /// <summary>
    /// Coroutine for loading the UI for battle scene. Instantiates actors for the selected hero team.
    /// </summary>
    /// <param name="sceneToMergeTo"></param>
    /// <returns></returns>
    private void SetUpBattleScene(Scene sceneToMergeTo)
    {
        Scene scene = SceneManager.GetSceneByName("battleUI");
        SceneManager.MergeScenes(scene, sceneToMergeTo);
        SummonScrollWindow summonScroll = UIManager.Instance.SummonScrollWindow;

        HashSet<AbilityBase> abilitiesInUse = new HashSet<AbilityBase>();
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

        foreach (AbilityBase abilityBase in abilitiesInUse)
            Debug.Log(abilityBase.idName);

        ResourceManager.Instance.LoadSpritesToBeUsed(abilitiesInUse);
    }

    public static void SetTimescale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}