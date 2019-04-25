using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    protected Projectile projectilePrefab;

    public ProjectilePool ProjectilePool;

    [SerializeField]
    protected EnemyActor enemyPrefab;

    public EnemyPool EnemyPool;

    public PlayerStats PlayerStats;
    public bool isInBattle;
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
        for (int i = 0; i < 80; i ++)
        {
            Equipment equipment = Equipment.CreateRandomEquipment(100);
            equipment.SetRarity((RarityType)Random.Range(1, 4));
            equipment.RerollAffixesAtRarity();
            PlayerStats.equipmentInventory.Add(equipment);
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

    public void MoveToMainMenu()
    {
        isInBattle = false;
        SceneManager.UnloadSceneAsync("battleUI");
        SceneManager.UnloadSceneAsync(currentSceneName);
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
    }

    public void MoveToBattle(string sceneName)
    {
        isInBattle = true;
        currentSceneName = sceneName;
        StartCoroutine(LoadBattleRoutine(sceneName));
        SceneManager.LoadScene("loadingScene", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("mainMenu");

       
    }

    IEnumerator LoadBattleRoutine(string sceneName)
    {

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        
        

        yield return LoadBattleUI();
    }

    IEnumerator LoadBattleUI()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("battleUI", LoadSceneMode.Additive);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        SummonScrollWindow summonScroll = UIManager.Instance.SummonScrollWindow;
        foreach (HeroData data in PlayerStats.heroList)
        {
            GameObject actor = Instantiate(ResourceManager.Instance.HeroPrefab);
            data.InitHeroActor(actor);
            HeroActor heroActor = actor.GetComponent<HeroActor>();
            if (heroActor == null)
                continue;
            summonScroll.AddHeroActor(heroActor);
        }
        UIManager.Instance.LoadingScreen.endLoadingScreen = true;
    }
}