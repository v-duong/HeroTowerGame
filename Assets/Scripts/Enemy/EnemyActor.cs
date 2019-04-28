using UnityEngine;

public class EnemyActor : Actor
{
    public new EnemyData Data
    {
        get
        {
            return (EnemyData)base.Data;
        }
        private set
        {
            base.Data = value;
        }
    }

    [SerializeField]
    public int spawnerOriginIndex;

    [SerializeField]
    public int indexOfGoal;

    private int nextMovementNode;

    public bool isBoss = false;

    // Use this for initialization
    public void Start()
    {
        Data = new EnemyData
        {
            MaximumHealth = 355,
            movementSpeed = 5
        };
        Data.CurrentHealth = Data.MaximumHealth;

        nextMovementNode = 1;
        InitializeHealthBar();
    }

    // Update is called once per frame
    public void Update()
    {
        if (this.gameObject.activeSelf)
        {
            Move();
            healthBar.UpdatePosition(this.transform);
        }
    }

    private void Move()
    {
        var dt = Time.deltaTime;
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        if (nodes != null && nextMovementNode < nodes.Count)
        {
            float dist = Vector3.Distance(nodes[nextMovementNode], this.transform.position);

            this.transform.position = Vector3.MoveTowards(this.transform.position, nodes[nextMovementNode], Data.movementSpeed * dt * actorTimeScale);

            if (dist <= 1.5f * Data.movementSpeed * dt)
            {
                nextMovementNode++;
            }
        }
    }

    public Vector3? GetMovementNode(int lookahead)
    {
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        int index = nextMovementNode + lookahead;
        if (index > nodes.Count-1)
        {
            return null;
        }
        return nodes[index];
    }

    public override void Death()
    {
        StageManager.Instance.WaveManager.enemiesSpawned -= 1;
        StageManager.Instance.WaveManager.currentEnemyList.Remove(this);
        
        this.gameObject.SetActive(false);
        this.healthBar.gameObject.SetActive(false);
    }

    public override ActorType GetActorType()
    {
        return ActorType.ENEMY;
    }

    [SerializeField]
    public Spawner ParentSpawner;
}