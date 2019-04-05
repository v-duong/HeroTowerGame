using UnityEngine;

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

    private WeightList<ConsumableType> consumableWeightList;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        ProjectilePool = new ProjectilePool(projectilePrefab);
        isInBattle = false;

#if !UNITY_EDITOR
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);

#endif
        PlayerStats = new PlayerStats();
        for (int i = 0; i < 20; i ++)
        {
            Equipment equipment = ResourceManager.Instance.CreateRandomEquipment(100);
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
            //consumableWeightList.Add(ConsumableType.RESET_NORMAL, 500);
            //consumableWeightList.Add(ConsumableType.VALUE_REROLL, 0);

        }

        return consumableWeightList.ReturnWeightedRandom();
    }
}