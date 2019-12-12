using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HeroActor : Actor
{
    public const float BASE_RECALL_TIME = 3f;
    private List<Vector3> movementNodes;
    private int deathCount;
    public bool isBeingRecalled;
    public Coroutine recallCoroutine;
    public float RecallTimer { get; private set; }
    protected List<ActorAbility> soulAbilities = new List<ActorAbility>();

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

    protected override void Update()
    {
        foreach(ActorAbility soulAbility in soulAbilities)
        {
            if (soulAbility.currentSoulCooldownTimer > 0)
                soulAbility.currentSoulCooldownTimer -= Time.deltaTime;
        }
        base.Update();
    }

    public void Initialize(HeroData data)
    {
        Data = data;
        NextMovementNode = 1;
        deathCount = 0;
        isBeingRecalled = false;
        recallCoroutine = null;
        movementNodes = new List<Vector3>();
        targetingPriority = PrimaryTargetingType.FIRST;

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
        if (data.GetAbilityFromSlot(2) != null)
        {
            ActorAbility soulAbility = data.GetAbilityFromSlot(2);
            if (soulAbility.abilityBase.isSoulAbility)
            {
                soulAbility.SetAbilityOwner(this);
                soulAbilities.Add(soulAbility);
            }
        }
    }

    public ActorAbility GetSoulAbility()
    {
        return Data.GetAbilityFromSlot(2);
    }

    public void ClearMovement()
    {
        movementNodes.Clear();
        NextMovementNode = 0;
    }

    protected override void Move()
    {
        if (isBeingRecalled)
        {
            ClearMovement();
            return;
        }

        var dt = Time.deltaTime;
        if (movementNodes != null && NextMovementNode < movementNodes.Count)
        {
            Vector3 destination = movementNodes[NextMovementNode] - transform.position;
            float dist = destination.sqrMagnitude;

            transform.position = Vector3.MoveTowards(transform.position, movementNodes[NextMovementNode], Data.movementSpeed * dt * actorTimeScale);

            float xDiff = movementNodes[NextMovementNode].x - this.transform.position.x;

            if (xDiff > 0)
                GetComponent<SpriteRenderer>().flipX = true;
            else if (xDiff < 0)
                GetComponent<SpriteRenderer>().flipX = false;

            if (dist <= 0.15f * Data.movementSpeed * dt)
            {
                NextMovementNode++;
            }
        }
        else
        {
            IsMoving = false;
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
            IsMoving = true;
            NextMovementNode = 0;
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
                NextMovementNode = 1;
                IsMoving = true;
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

    private void ClearHeroTemporaryValues()
    {
        Data.ClearTemporaryBonuses(true);
        ClearStatusEffects(true);
        ClearMovement();
    }

    public override void Death()
    {
        Data.OnHitData.ApplyTriggerEffects(TriggerType.ON_DEATH, this);

        ClearHeroTemporaryValues();
        StopCurrentRecall();

        UIManager.Instance.SummonScrollWindow.UnsummonHero(this, 10f + 5f * deathCount, true);
        StageManager.Instance.BattleManager.activeHeroes.Remove(this);
        DisableActor();

        deathCount++;
    }

    public void StopCurrentRecall()
    {
        if (recallCoroutine != null)
        {
            StopCoroutine(recallCoroutine);
            recallCoroutine = null;
        }
        isBeingRecalled = false;
        RecallTimer = 0;
    }

    public void SummonHero()
    {
        ClearMovement();
        EnableHealthBar();
        gameObject.SetActive(true);
    }

    public void UnsummonHero()
    {
        ClearMovement();
        StopCurrentRecall();
        isBeingRecalled = true;
        recallCoroutine = StartCoroutine(RecallCoroutine());
    }

    public IEnumerator RecallCoroutine()
    {
        while (RecallTimer < BASE_RECALL_TIME)
        {
            if (!StageManager.Instance.BattleManager.startedSpawn)
                RecallTimer += BASE_RECALL_TIME;
            RecallTimer += Time.deltaTime;
            yield return null;
        }
        isBeingRecalled = false;
        RecallTimer = 0;
        ClearHeroTemporaryValues();

        if (!StageManager.Instance.BattleManager.startedSpawn)
            UIManager.Instance.SummonScrollWindow.UnsummonHero(this, 0.25f, false);
        else
            UIManager.Instance.SummonScrollWindow.UnsummonHero(this, 2f, false);

        StageManager.Instance.BattleManager.activeHeroes.Remove(this);
        DisableActor();
        recallCoroutine = null;
    }

    public override ActorType GetActorType()
    {
        return ActorType.ALLY;
    }
}