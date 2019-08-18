using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerStats PlayerStats { get; private set; }

    [SerializeField]
    protected Projectile projectilePrefab;

    public ProjectilePool ProjectilePool;

    public bool isInBattle;
    public List<HeroData> inBattleHeroes = new List<HeroData>();
    private string currentSceneName = "";

    private WeightList<ConsumableType> consumableWeightList;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        ProjectilePool = new ProjectilePool(projectilePrefab);
        isInBattle = false;
        currentSceneName = "mainMenu";

#if !UNITY_EDITOR
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
#endif

        PlayerStats = new PlayerStats();
        for (int i = 0; i < 80; i++)
        {
            Equipment equipment = Equipment.CreateRandomEquipment(100);
            equipment.SetRarity((RarityType)Random.Range(1, 4));
            equipment.RerollAffixesAtRarity();
            PlayerStats.AddEquipmentToInventory(equipment);
        }
    }

    public ConsumableType GetRandomConsumable()
    {
        if (consumableWeightList == null)
        {
            consumableWeightList = new WeightList<ConsumableType>();
            consumableWeightList.Add(ConsumableType.NORMAL_TO_MAGIC, 750);
            consumableWeightList.Add(ConsumableType.MAGIC_REROLL, 1500);
            consumableWeightList.Add(ConsumableType.NORMAL_TO_RARE, 400);
            consumableWeightList.Add(ConsumableType.MAGIC_TO_RARE, 600);
            consumableWeightList.Add(ConsumableType.RARE_REROLL, 900);
            consumableWeightList.Add(ConsumableType.RARE_TO_EPIC, 20);
            consumableWeightList.Add(ConsumableType.ADD_AFFIX, 500);
            consumableWeightList.Add(ConsumableType.REMOVE_AFFIX, 250);
            //consumableWeightList.Add(ConsumableType.RESET_NORMAL, 0);
            //consumableWeightList.Add(ConsumableType.VALUE_REROLL, 0);
        }
        return consumableWeightList.ReturnWeightedRandom();
    }

    public void AddRandomConsumableToInventory()
    {
        PlayerStats.consumables[GetRandomConsumable()] += 1;
    }

    public void MoveToMainMenu()
    {
        ProjectilePool.ReturnAll();
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
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        StageManager.Instance.HighlightMap.gameObject.SetActive(false);
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        StageManager.Instance.BattleManager.SetStageBase(stageInfoBase);
        ParticleManager.Instance.ClearParticleSystems();

        yield return LoadBattleUI(scene);
    }

    /// <summary>
    /// Coroutine for loading the UI for battle scene. Instantiates actors for the selected hero team.
    /// </summary>
    /// <param name="sceneToMergeTo"></param>
    /// <returns></returns>
    private IEnumerator LoadBattleUI(Scene sceneToMergeTo)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("battleUI", LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        Scene scene = SceneManager.GetSceneByName("battleUI");
        SceneManager.MergeScenes(scene, sceneToMergeTo);
        SummonScrollWindow summonScroll = UIManager.Instance.SummonScrollWindow;

        inBattleHeroes.Clear();

        foreach (HeroData data in PlayerStats.heroTeams[0])
        {
            if (data == null)
                continue;
            GameObject actor = Instantiate(ResourceManager.Instance.HeroPrefab.gameObject);
            data.InitHeroActor(actor);
            HeroActor heroActor = actor.GetComponent<HeroActor>();
            if (heroActor == null)
                continue;
            summonScroll.AddHeroActor(heroActor);
            inBattleHeroes.Add(data);
        }

        StageManager.Instance.DisplayMap.CompressBounds();
        Bounds bounds = StageManager.Instance.DisplayMap.localBounds;
        InputManager.Instance.SetCameraBounds(bounds);

        UIManager.Instance.LoadingScreen.endLoadingScreen = true;
        isInBattle = true;
    }
}