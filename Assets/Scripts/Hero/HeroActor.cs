using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HeroActor : Actor
{
    private List<Vector3> movementNodes;

    public new HeroData Data
    {
        get
        {
            return (HeroData)base.Data;
        }
        private set
        {
            base.Data = value;
        }
    }

    public void Initialize(HeroData data)
    {
        Data = data;
        nextMovementNode = 1;
        movementNodes = new List<Vector3>();
        if (data.GetAbilityFromSlot(0) != null)
        {
            ActorAbility firstAbility = data.GetAbilityFromSlot(0);
            AddAbilityToList(firstAbility);
        }
        if (data.GetAbilityFromSlot(1) != null)
        {
            ActorAbility secondAbility = data.GetAbilityFromSlot(1);
            AddAbilityToList(secondAbility);
        }
    }

    protected override void Move()
    {
        var dt = Time.deltaTime;
        if (movementNodes != null && nextMovementNode < movementNodes.Count)
        {
            Vector3 vector = movementNodes[nextMovementNode] - transform.position;
            float dist = vector.sqrMagnitude;

            transform.position = Vector3.MoveTowards(transform.position, movementNodes[nextMovementNode], Data.movementSpeed * dt * actorTimeScale);

            if (dist <= 0.15f * Data.movementSpeed * dt)
            {
                nextMovementNode++;
            }
        }
        else
        {
            isMoving = false;
        }
    }

    public void StartMovement(Vector3 destination)
    {
        destination = Helpers.ReturnTilePosition(StageManager.Instance.HighlightMap.tilemap, destination, -3);
        Vector3 vector = destination - transform.position;
        float dist = vector.sqrMagnitude;

        if (dist <= 0.0f)
        {
            movementNodes.Clear();
            movementNodes.Add(destination);
            isMoving = true;
            nextMovementNode = 0;
        }
        else
        {
            Tilemap tilemap = StageManager.Instance.HighlightMap.tilemap;
            Vector3Int cellPos = tilemap.WorldToCell(transform.position);
            Vector3 startPos = tilemap.GetCellCenterWorld(cellPos);
            startPos.z = -3;
            movementNodes = Pathfinding.FindPath(startPos, destination, StageManager.Instance.HighlightMap.tilemap, true);
            if (movementNodes != null)
            {
                nextMovementNode = 1;
                isMoving = true;
            }
        }
    }

    private void OnEnable()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StartFiring(this);
        }
    }

    private void OnDisable()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }
    }

    public override void Death()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }

        DisableActor();
    }

    public override ActorType GetActorType()
    {
        return ActorType.ALLY;
    }
}

public enum TargetingPriorityType
{
    CLOSEST,
    FURTHEST,
    FIRST,
    RANDOM,
    LEAST_HEALTH,
    MOST_HEALTH,
    PRIORITIZE_RARITY
}