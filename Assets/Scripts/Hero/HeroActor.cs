﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HeroActor : Actor
{
    private bool isMoving = false;
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
            this.AddAbilityToList(firstAbility);
        }
        if (data.GetAbilityFromSlot(1) != null)
        {
            ActorAbility secondAbility = data.GetAbilityFromSlot(1);
            this.AddAbilityToList(secondAbility);
        }
    }

    void Move()
    {
        var dt = Time.deltaTime;
        if (movementNodes != null && nextMovementNode < movementNodes.Count)
        {
            Vector3 vector = movementNodes[nextMovementNode] - transform.position;
            float dist = vector.sqrMagnitude;

            this.transform.position = Vector3.MoveTowards(this.transform.position, movementNodes[nextMovementNode], Data.movementSpeed * dt * actorTimeScale);

            if (dist <= 0.35f * Data.movementSpeed * dt)
            {
                nextMovementNode++;
            }
        } else
        {
            isMoving = false;
        }
    }

    public void StartMovement(Vector3 destination)
    {
        Vector3 vector = destination - transform.position;
        float dist = vector.sqrMagnitude;

        if (dist <= 2f)
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

    void Update()
    {
        if (isMoving)
        {
            Move();
            //healthBar.UpdatePosition(transform);
        }
    }

    void OnEnable()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StartFiring(this);
        }
    }

    void OnDisable()
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
    }

    public override ActorType GetActorType()
    {
        return ActorType.ALLY;
    }
}
