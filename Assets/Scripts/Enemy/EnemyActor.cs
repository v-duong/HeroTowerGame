﻿using UnityEngine;

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

    public bool isBoss = false;

    public EnemyActor()
    {
        Data = new EnemyData
        {
            MaximumHealth = 100,
            movementSpeed = 5
        };
        Data.CurrentHealth = Data.MaximumHealth;
    }

    // Use this for initialization
    private void Start()
    {
        nextMovementNode = 1;
        InitializeHealthBar();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateStatusEffects();
        if (!this.gameObject.activeSelf)
            return;
        Move();
        healthBar.UpdatePosition(this.transform);
    }

    public void SetBase(EnemyBase enemyBase)
    {
        Data.SetBase(enemyBase);
    }

    private void Move()
    {
        var dt = Time.deltaTime;
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        if (nodes != null && nextMovementNode < nodes.Count)
        {
            //float dist = Vector3.Distance(nodes[nextMovementNode], this.transform.position);
            float dist = Vector3.SqrMagnitude(nodes[nextMovementNode] - this.transform.position);

            this.transform.position = Vector3.MoveTowards(this.transform.position, nodes[nextMovementNode], Data.movementSpeed * dt * actorTimeScale);

            if (dist <= 0.1f * Data.movementSpeed * Data.movementSpeed * dt)
            {
                nextMovementNode++;
            }
        }
    }

    public Vector3? GetMovementNode(int lookahead)
    {
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        int index = nextMovementNode + lookahead;
        if (index > nodes.Count - 1)
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