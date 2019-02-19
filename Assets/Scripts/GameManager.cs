using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField]
    protected Projectile projectilePrefab;
    public ProjectilePool ProjectilePool;

    [SerializeField]
    protected EnemyActor enemyPrefab;
    public EnemyPool EnemyPool;

    public PlayerStats PlayerStats;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        ProjectilePool = new ProjectilePool(projectilePrefab);
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Additive);
        PlayerStats = new PlayerStats();
    }

}
