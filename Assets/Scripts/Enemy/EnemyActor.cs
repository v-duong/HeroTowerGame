using System.Collections.Generic;
using UnityEngine;

public class EnemyActor : Actor
{
    public RarityType enemyRarity;
    public List<Affix> mobAffixes;

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
        isMoving = true;
        Data = new EnemyData();
        Data.CurrentHealth = Data.MaximumHealth;
        mobAffixes = new List<Affix>();
    }

    // Use this for initialization
    private new void Start()
    {
        base.Start();

        nextMovementNode = 1;
    }

    protected override void Move()
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
        else
        {
            StageManager.Instance.BattleManager.playerHealth--;
            Death();
        }
    }

    public void SetBase(EnemyBase enemyBase, RarityType rarity, int level)
    {
        enemyRarity = rarity;
        Data.SetBase(enemyBase, rarity, level);
        if (rarity == RarityType.RARE)
        {
            AddRandomMobAffixes(3);
        }
        else if (rarity == RarityType.UNCOMMON)
        {
            AddRandomMobAffixes(1);
        }
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
            actorAbility.SetAbilityOwner(this);
            AddAbilityToList(actorAbility);
            actorAbility.UpdateAbilityStats(Data);
            actorAbility.StartFiring(this);
        }
    }

    public void AddRandomMobAffixes(int affixCount)
    {
        List<string> bonusTags = new List<string>();
        foreach (Affix a in mobAffixes)
            bonusTags.Add(a.Base.BonusTagType);
        for (int i = 0; i < affixCount; i++)
        {
            Affix affix = new Affix(ResourceManager.Instance.GetRandomAffixBase(AffixType.MONSTERMOD, Data.Level, null, bonusTags));
            mobAffixes.Add(affix);
            bonusTags.Add(affix.Base.BonusTagType);
            foreach (AffixBonusProperty prop in affix.Base.affixBonuses)
            {
                Data.AddStatBonus(affix.GetAffixValue(prop.bonusType), prop.bonusType, prop.modifyType);
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

        foreach (var x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }

        DisableActor();
    }

    public override ActorType GetActorType()
    {
        return ActorType.ENEMY;
    }

    [SerializeField]
    public Spawner ParentSpawner;
}