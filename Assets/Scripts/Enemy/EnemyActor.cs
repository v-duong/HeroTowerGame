using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActor : Actor {
    private int nextMovementNode;
    [SerializeField]
    public int spawnerOriginIndex;
    [SerializeField]
    public int indexOfGoal;

    public bool isBoss = false;

	// Use this for initialization
	public override void Start () {
        nextMovementNode = 1;
        InitializeHealthBar();
    }
	
	// Update is called once per frame
	public void Update () {
        Move();
        healthBar.UpdatePosition(this.transform);
    }

    private void Move()
    {
        var dt = Time.deltaTime;
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        if (nodes != null && nextMovementNode < nodes.Count)
        {
            float dist = Vector3.Distance(nodes[nextMovementNode], this.transform.position);

            this.transform.position = Vector3.MoveTowards(this.transform.position, nodes[nextMovementNode], movementSpeed * dt * actorTimeScale);

            if (dist <= 1.5f * movementSpeed * dt)
            {
                nextMovementNode++;
            }
        }
    }

    public override void Death()
    {
        StageManager.Instance.WaveManager.enemiesSpawned -= 1;
        StageManager.Instance.WaveManager.currentEnemyList.Remove(this);
        this.healthBar.transform.gameObject.SetActive(false);
        this.transform.gameObject.SetActive(false);
    }

    [SerializeField]
    public Spawner ParentSpawner;
}
