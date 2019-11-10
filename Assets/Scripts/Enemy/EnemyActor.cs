using System.Collections.Generic;
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
        NextMovementNode = 1;
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
        if (nodes != null && NextMovementNode < nodes.Count)
        {
            //float dist = Vector3.Distance(nodes[nextMovementNode], this.transform.position);
            Vector3 destination = nodes[NextMovementNode] + rotatedOffset;

            transform.position = Vector3.MoveTowards(transform.position, destination, Data.movementSpeed * dt * actorTimeScale);

            float dist = Vector3.SqrMagnitude(destination - transform.position);

            if (dist <= 0.1f * Data.movementSpeed * Data.movementSpeed * dt)
            {
                NextMovementNode++;
                CalculateRotatedOffset();
            }

            if (NextMovementNode < nodes.Count)
            {
                distanceToNextNode = Vector3.SqrMagnitude(nodes[NextMovementNode] - transform.position);

                Vector3 nextDestination = nodes[NextMovementNode] + rotatedOffset;

                float xDiff = nextDestination.x - transform.position.x;

                if (xDiff > 0)
                    GetComponentInChildren<SpriteRenderer>().flipX = true;
                else if (xDiff < 0)
                    GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
        }
        else
        {
            Debug.Log("END PATH");
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
        if (Data.BaseEnemyData.enemyType == EnemyType.TARGET_ATTACKER)
        {
            foreach (ActorAbility ability in Data.abilities)
            {
                if ((ability.abilityBase.abilityType == AbilityType.ATTACK || ability.abilityBase.abilityType == AbilityType.SPELL) && ability.targetList.Count > 0)
                {
                    if (ability.targetList.FindAll(x => Vector3.Distance(transform.position, x.transform.position) <= ability.TargetRange).Count > 0)
                    {
                        isMoving = false;
                        return;
                    }
                }
            }

            isMoving = true;
        }
    }

    public void CalculateRotatedOffset()
    {
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        Vector3 nextPos, heading;
        float angle;
        if (NextMovementNode + 1 < nodes.Count)
        {
            nextPos = nodes[NextMovementNode + 1];
            heading = nextPos - nodes[NextMovementNode];
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
                Data.AddStatBonus(BonusType.MAX_HEALTH, GroupType.NO_GROUP, ModifyType.MULTIPLY, 1200);
                Data.AddStatBonus(BonusType.GLOBAL_DAMAGE, GroupType.NO_GROUP, ModifyType.MULTIPLY, 75);
                sizeScaling *= 1.28f;
            }
            propertyBlock=  ResourceManager.Instance.bossMaterialBlock;
        }
        else if (rarity == RarityType.RARE)
        {
            Data.AddStatBonus(BonusType.MAX_HEALTH, GroupType.NO_GROUP, ModifyType.MULTIPLY, 500);
            Data.AddStatBonus(BonusType.GLOBAL_DAMAGE, GroupType.NO_GROUP, ModifyType.MULTIPLY, 30);
            this.transform.localScale = new Vector3(1.14f * enemyBase.sizeScaling, 1.14f * enemyBase.sizeScaling);
            sizeScaling *= 1.14f;
            AddRandomStatAffixes(3);
            propertyBlock = ResourceManager.Instance.rareMaterialBlock;
        }
        else if (rarity == RarityType.UNCOMMON)
        {
            Data.AddStatBonus(BonusType.MAX_HEALTH, GroupType.NO_GROUP, ModifyType.MULTIPLY, 200);
            Data.AddStatBonus(BonusType.GLOBAL_DAMAGE, GroupType.NO_GROUP, ModifyType.MULTIPLY, 10);
            AddRandomStatAffixes(1);
            propertyBlock = ResourceManager.Instance.uncommonMaterialBlock;
        } else
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
        var nodes = ParentSpawner.GetNodesToGoal(indexOfGoal);
        int index = NextMovementNode + lookahead;
        if (index > nodes.Count - 1)
        {
            return null;
        }
        return nodes[index];
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