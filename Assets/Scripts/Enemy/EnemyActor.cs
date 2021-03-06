﻿using System.Collections.Generic;
using UnityEngine;

public class EnemyActor : Actor
{
    public RarityType enemyRarity;
    public List<Affix> mobAffixes;
    public Vector3 positionOffset;
    public Vector3 rotatedOffset;
    public float distanceToNextNode = 0f;

    private bool skippedAngleChange = false;
    private Vector3 previousHeading;
    private List<Vector3> currentPath;
    private Actor followTarget;

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

    public List<Vector3> CurrentPath
    {
        get
        {
            if (currentPath == null)
                currentPath = ParentSpawner.GetNodesToGoal(indexOfGoal);
            return currentPath;
        }
    }

    public EnemyActor()
    {
        positionOffset = Vector3.zero;
        IsMoving = true;
        InitData();
        mobAffixes = new List<Affix>();
        previousHeading = Vector3.up;
    }

    public void InitData()
    {
        Data = new EnemyData();
        Data.CurrentHealth = Data.MaximumHealth;
    }

    // Use this for initialization
    public void Init(int goalIndex)
    {
        IsMoving = true;
        currentPath = null;
        indexOfGoal = goalIndex;
        NextMovementNode = 1;
        Data.CurrentHealth = Data.MaximumHealth;
        Data.CurrentManaShield = Data.MaximumManaShield;
        followTarget = null;
        CalculateRotatedOffset();
        EnableHealthBar();
        InitializeHealthBar();
    }

    protected override void Move()
    {
        var dt = Time.deltaTime;
        float movementSpeed;
        if (CurrentPath != null && NextMovementNode < CurrentPath.Count)
        {
            if (followTarget == null)
            {
                movementSpeed = Data.movementSpeed;
            }
            else
            {
                movementSpeed = followTarget.Data.movementSpeed;
            }

            //float dist = Vector3.Distance(currentPath[nextMovementNode], this.transform.position);
            Vector3 destination = CurrentPath[NextMovementNode] + rotatedOffset;

            transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed * dt * actorTimeScale);

            float dist = Vector3.SqrMagnitude(destination - transform.position);

            if (dist <= 0.13f * movementSpeed * dt)
            {
                NextMovementNode++;
                CalculateRotatedOffset();
            }

            if (NextMovementNode < CurrentPath.Count)
            {
                distanceToNextNode = Vector3.SqrMagnitude(CurrentPath[NextMovementNode] - transform.position);

                Vector3 nextDestination = CurrentPath[NextMovementNode] + rotatedOffset;

                float xDiff = nextDestination.x - transform.position.x;

                if (xDiff > 0)
                    GetComponentInChildren<SpriteRenderer>().flipX = true;
                else if (xDiff < 0)
                    GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
        }
        else
        {
            switch (enemyRarity)
            {
                case RarityType.NORMAL when isBoss:
                    StageManager.Instance.BattleManager.ModifyPlayerHealth(-10, true);
                    break;

                case RarityType.NORMAL:
                    StageManager.Instance.BattleManager.ModifyPlayerHealth(-1, true);
                    break;

                case RarityType.UNCOMMON:
                    StageManager.Instance.BattleManager.ModifyPlayerHealth(-2, true);
                    break;

                case RarityType.RARE:
                    StageManager.Instance.BattleManager.ModifyPlayerHealth(-5, true);
                    break;

                default:
                    StageManager.Instance.BattleManager.ModifyPlayerHealth(-1, true);
                    break;
            }
            Death();
        }
    }

    private void LateUpdate()
    {
        switch (Data.BaseEnemyData.enemyType)
        {
            case EnemyType.NON_ATTACKER:
                break;

            case EnemyType.TARGET_ATTACKER:
                foreach (ActorAbility ability in Data.abilities)
                {
                    if ((ability.abilityBase.abilityType == AbilityType.ATTACK || ability.abilityBase.abilityType == AbilityType.SPELL) && ability.targetList.Count > 0)
                    {
                        /*
                        float adjustedRange = ability.TargetRange * this.transform.localScale.x;
                        if (ability.targetList.FindAll(x => Vector2.Distance(transform.position, x.transform.position) <= adjustedRange).Count > 0)
                        {
                            IsMoving = false;
                            return;
                        }
                        */
                        IsMoving = false;
                        return;
                    }
                }
                IsMoving = true;
                return;

            case EnemyType.HIT_AND_RUN:
                return;

            case EnemyType.AURA_USER:
                if (followTarget == null || followTarget.Data.IsDead)
                {
                    Actor slowestTarget = null;
                    float slowestSpeed = float.PositiveInfinity;
                    foreach (ActorAbility ability in Data.abilities)
                    {
                        if ((ability.abilityBase.abilityType == AbilityType.AURA) && ability.targetList.Count > 0)
                        {
                            foreach(Actor actor in ability.targetList)
                            {
                                if (actor.Data.movementSpeed < slowestSpeed)
                                {
                                    slowestTarget = actor;
                                    slowestSpeed = actor.Data.movementSpeed;
                                }
                            }
                        }
                    }

                    followTarget = slowestTarget;
                }
                return;

            case EnemyType.DEBUFFER:
                return;
        }
    }

    public void CalculateRotatedOffset()
    {
        Vector3 nextPos, heading;
        float angle;
        if (NextMovementNode + 1 < CurrentPath.Count)
        {
            nextPos = CurrentPath[NextMovementNode + 1];
            heading = nextPos - CurrentPath[NextMovementNode];
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

        targetingPriority = enemyBase.targetingPriority;

        float sizeScaling = enemyBase.sizeScaling;
        MaterialPropertyBlock propertyBlock;

        if (isBoss)
        {
            if (!enemyBase.isBoss)
            {
                Data.AddStatBonus(BonusType.MAX_HEALTH, GroupType.NO_GROUP, ModifyType.MULTIPLY, 1100);
                Data.AddStatBonus(BonusType.GLOBAL_DAMAGE, GroupType.NO_GROUP, ModifyType.MULTIPLY, 70);
                Data.AddStatBonus(BonusType.AFFLICTED_STATUS_THRESHOLD, GroupType.NO_GROUP, ModifyType.MULTIPLY, -50f);
                sizeScaling *= 1.28f;
            }
            propertyBlock = ResourceManager.Instance.bossMaterialBlock;
        }
        else if (rarity == RarityType.RARE)
        {
            Data.AddStatBonus(BonusType.MAX_HEALTH, GroupType.NO_GROUP, ModifyType.MULTIPLY, 500);
            Data.AddStatBonus(BonusType.GLOBAL_DAMAGE, GroupType.NO_GROUP, ModifyType.MULTIPLY, 30);
            Data.AddStatBonus(BonusType.AFFLICTED_STATUS_THRESHOLD, GroupType.NO_GROUP, ModifyType.MULTIPLY, -50f);
            this.transform.localScale = new Vector3(1.14f * enemyBase.sizeScaling, 1.14f * enemyBase.sizeScaling);
            sizeScaling *= 1.14f;
            AddRandomStatAffixes(3);
            propertyBlock = ResourceManager.Instance.rareMaterialBlock;
        }
        else if (rarity == RarityType.UNCOMMON)
        {
            Data.AddStatBonus(BonusType.MAX_HEALTH, GroupType.NO_GROUP, ModifyType.MULTIPLY, 200);
            Data.AddStatBonus(BonusType.GLOBAL_DAMAGE, GroupType.NO_GROUP, ModifyType.MULTIPLY, 10);
            Data.AddStatBonus(BonusType.AFFLICTED_STATUS_THRESHOLD, GroupType.NO_GROUP, ModifyType.MULTIPLY, -25f);
            AddRandomStatAffixes(1);
            propertyBlock = ResourceManager.Instance.uncommonMaterialBlock;
        }
        else
        {
            propertyBlock = ResourceManager.Instance.normalMaterialBlock;
        }

        propertyBlock.SetTexture("_MainTex", GetComponent<SpriteRenderer>().sprite.texture);
        //GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
        GetComponent<SpriteRenderer>().SetPropertyBlock(propertyBlock);

        this.transform.localScale = Vector3.one * sizeScaling;

        instancedAbilitiesList.Clear();
        foreach (EnemyBase.EnemyAbilityBase ability in enemyBase.abilitiesList)
        {
            AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(ability.abilityName);

            if (Data.BaseEnemyData.enemyType == EnemyType.NON_ATTACKER && abilityBase.abilityType != AbilityType.AURA && abilityBase.abilityType != AbilityType.SELF_BUFF)
                continue;

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
            actorAbility.abilityCollider.transform.localScale = Vector3.one / sizeScaling;
        }
    }

    public void AddRandomStatAffixes(int affixCount)
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
        int index = NextMovementNode + lookahead;
        if (index > CurrentPath.Count - 1)
        {
            return null;
        }
        return CurrentPath[index];
    }

    public override void Death()
    {
        Data.OnHitData.ApplyTriggerEffects(TriggerType.ON_DEATH, this);

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