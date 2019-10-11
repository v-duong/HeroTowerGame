using System.Collections.Generic;
using UnityEngine;

public class EnemyActor : Actor
{
    public RarityType enemyRarity;
    public List<Affix> mobAffixes;
    public Vector3 positionOffset;
    public Vector3 rotatedOffset;

    private bool skippedAngleChange = false;
    private Vector3 previousHeading;

    [SerializeField]
    public int spawnerOriginIndex;

    [SerializeField]
    public int indexOfGoal;

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

    public EnemyActor()
    {
        positionOffset = Vector3.zero;
        isMoving = true;
        Data = new EnemyData();
        Data.CurrentHealth = Data.MaximumHealth;
        mobAffixes = new List<Affix>();
        previousHeading = Vector3.up;
    }

    // Use this for initialization
    public void Init()
    {
        nextMovementNode = 1;
        Data.CurrentHealth = Data.MaximumHealth;
        Data.CurrentManaShield = Data.MaximumManaShield;
        CalculateRotatedOffset();
        EnableHealthBar();
        InitializeHealthBar();
    }

    protected override void Move()
    {
        var dt = Time.deltaTime;
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        if (nodes != null && nextMovementNode < nodes.Count)
        {
            //float dist = Vector3.Distance(nodes[nextMovementNode], this.transform.position);
            Vector3 destination = nodes[nextMovementNode] + rotatedOffset;

            transform.position = Vector3.MoveTowards(transform.position, destination, Data.movementSpeed * dt * actorTimeScale);

            float dist = Vector3.SqrMagnitude(destination - transform.position);

            if (dist <= 0.1f * Data.movementSpeed * Data.movementSpeed * dt)
            {
                nextMovementNode++;
                CalculateRotatedOffset();
            }
        }
        else
        {
            Debug.Log("END PATH");
            StageManager.Instance.BattleManager.playerHealth--;
            Death();
        }
    }

    public void CalculateRotatedOffset()
    {
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        Vector3 nextPos, heading;
        float angle;
        if (nextMovementNode + 1 < nodes.Count)
        {
            nextPos = nodes[nextMovementNode + 1];
            heading = nextPos - nodes[nextMovementNode];
            angle = Vector3.SignedAngle(previousHeading, heading.normalized, Vector3.forward);
            if (angle > 0 && !skippedAngleChange && positionOffset.x * positionOffset.y > 0 || angle < 0 && !skippedAngleChange && positionOffset.x * positionOffset.y < 0)
            {
                skippedAngleChange = true;
                return;
            }
        }
        else
        {
            return;
        }

        skippedAngleChange = false;
        previousHeading = heading.normalized;
        if (angle != 0)
        {
            rotatedOffset = Quaternion.Euler(0, 0, angle) * rotatedOffset;
        }
    }

    public void SetBase(EnemyBase enemyBase, RarityType rarity, int level)
    {
        enemyRarity = rarity;
        Data.SetBase(enemyBase, rarity, level, this);

        if (rarity == RarityType.RARE)
        {
            AddRandomMobAffixes(3);
        }
        else if (rarity == RarityType.UNCOMMON)
        {
            AddRandomMobAffixes(1);
        }

        instancedAbilitiesList.Clear();
        foreach (EnemyBase.EnemyAbilityBase ability in enemyBase.abilitiesList)
        {
            AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(ability.abilityName);

            int layer;
            if (abilityBase.targetType == AbilityTargetType.ENEMY)
            {
                layer = LayerMask.NameToLayer("AllyDetect");
            }
            else if (abilityBase.targetType == AbilityTargetType.ALLY)
            {
                layer = LayerMask.NameToLayer("EnemyDetect");
            }
            else
            {
                layer = LayerMask.NameToLayer("BothDetect");
            }

            ActorAbility actorAbility = new ActorAbility(abilityBase, layer);
            actorAbility.SetDamageAndSpeedModifier((ability.damageMultiplier - 1f) * 100f, (ability.attackPerSecMultiplier - 1f) * 100f);
            actorAbility.UpdateAbilityLevel(Data.GetAbilityLevel());
            actorAbility.SetAbilityOwner(this);
            AddAbilityToList(actorAbility);
            Data.abilities.Add(actorAbility);
            actorAbility.UpdateAbilityStats(Data);
            actorAbility.StartFiring(this);
        }
    }

    public void AddRandomMobAffixes(int affixCount)
    {
        List<string> bonusTags = new List<string>();
        foreach (Affix a in mobAffixes)
            bonusTags.Add(a.Base.AffixBonusTypeString);
        for (int i = 0; i < affixCount; i++)
        {
            Affix affix = new Affix(ResourceManager.Instance.GetRandomAffixBase(AffixType.MONSTERMOD, Data.Level, null, bonusTags));
            mobAffixes.Add(affix);
            bonusTags.Add(affix.Base.AffixBonusTypeString);
            for (int j = 0; j < affix.Base.affixBonuses.Count; j++)
            {
                AffixBonusProperty prop = affix.Base.affixBonuses[j];
                Data.AddStatBonus(prop.bonusType, prop.restriction, prop.modifyType, affix.GetAffixValue(j));
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
        StageManager.Instance.BattleManager.enemiesSpawned -= 1;
        StageManager.Instance.BattleManager.currentEnemyList.Remove(this);
        ClearStatusEffects(true);

        DisableActor();
        StageManager.Instance.BattleManager.EnemyPool.ReturnToPool(this);
    }

    public override ActorType GetActorType()
    {
        return ActorType.ENEMY;
    }

    [SerializeField]
    public Spawner ParentSpawner;
}