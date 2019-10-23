using System;
using System.Collections.Generic;

public class EnemyData : ActorData
{
    public EnemyBase BaseEnemyData { get; protected set; }
    protected Dictionary<BonusType, StatBonus> mobBonuses;
    public int minAttackDamage;
    public int maxAttackDamage;
    public List<ActorAbility> abilities;

    public EnemyData() : base()
    {
        Id = Guid.NewGuid();
        BaseManaShield = 0;
        CurrentManaShield = 0;
        BaseSoulPoints = 0;
        CurrentSoulPoints = 0;

        abilities = new List<ActorAbility>();
        mobBonuses = new Dictionary<BonusType, StatBonus>();
    }

    public void ClearData()
    {
        mobBonuses.Clear();
    }

    public void SetBase(EnemyBase enemyBase, RarityType rarity, int level, EnemyActor actor)
    {
        BaseEnemyData = enemyBase;
        Name = enemyBase.idName;
        Level = level;
        CurrentActor = actor;
        OnHitData.SourceActor = actor;
        BaseHealth = (float)(Helpers.GetEnemyHealthScaling(level) * enemyBase.healthScaling);
        MaximumHealth = (int)BaseHealth;
        CurrentHealth = MaximumHealth;
        movementSpeed = enemyBase.movementSpeed;
        minAttackDamage = (int)(enemyBase.attackDamageMinMultiplier * Helpers.GetEnemyDamageScaling(level));
        maxAttackDamage = (int)(enemyBase.attackDamageMaxMultiplier * Helpers.GetEnemyDamageScaling(level));
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            ElementData[element] = enemyBase.resistances[(int)element];
        }

        CurrentShieldDelay = 0f;
    }

    public override void UpdateActorData()
    {
        BaseManaShield = 0;
        GroupTypes = GetGroupTypes();
        ApplyHealthBonuses();
        ApplySoulPointBonuses();

        float extraManaShield = CurrentHealth * GetMultiStatBonus(GroupTypes, BonusType.HEALTH_AS_EXTRA_SHIELD).CalculateStat(0) / 100f;
        MaximumManaShield = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_MAX_SHIELD).CalculateStat(BaseManaShield + (int)extraManaShield), 0);

        if (MaximumManaShield != 0)
        {
            float shieldPercent = CurrentManaShield / MaximumManaShield;
            CurrentManaShield = MaximumManaShield * shieldPercent;
        }

        foreach (ActorAbility ability in abilities)
        {
            ability.UpdateAbilityStats(this);
        }

        movementSpeed = GetMultiStatBonus(GroupTypes, BonusType.MOVEMENT_SPEED).CalculateStat(BaseEnemyData.movementSpeed);

        Armor = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_ARMOR).CalculateStat(BaseArmor), 0);
        DodgeRating = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_DODGE_RATING).CalculateStat(BaseDodgeRating), 0);
        ResolveRating = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_RESOLVE_RATING).CalculateStat(BaseResolveRating), 0);
        AttackPhasing = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.ATTACK_PHASING).CalculateStat(BaseAttackPhasing), 0);
        MagicPhasing = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.MAGIC_PHASING).CalculateStat(BaseMagicPhasing), 0);

        AfflictedStatusDamageResistance = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_DAMAGE_RESISTANCE).CalculateStat(0f), 90) / 100f;
        AfflictedStatusDamageResistance = 1f - AfflictedStatusDamageResistance;
        AfflictedStatusThreshold = Math.Max(GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_THRESHOLD).CalculateStat(1f), 0.01f);

        float BlockChanceCap = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.MAX_SHIELD_BLOCK_CHANCE).CalculateStat(BLOCK_CHANCE_CAP), 100f);
        float BlockProtectionCap = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.MAX_SHIELD_BLOCK_PROTECTION).CalculateStat(BLOCK_PROTECTION_CAP), 100f);

        BlockChance = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.SHIELD_BLOCK_CHANCE).CalculateStat(0f), BlockChanceCap) / 100f;
        BlockProtection = Math.Min(GetMultiStatBonus(GroupTypes, BonusType.SHIELD_BLOCK_PROTECTION).CalculateStat(0f), BlockProtectionCap) / 100f;

        base.UpdateActorData();
    }

    public override void GetTotalStatBonus(BonusType type, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus inputBonus)
    {
        StatBonus resultBonus;
        if (inputBonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = inputBonus;

        List<StatBonus> bonuses = new List<StatBonus>();

        if (statBonuses.TryGetValue(type, out StatBonusCollection statBonus))
        {
            bonuses.Add(statBonus.GetTotalStatBonus(tags));
        }
        if (mobBonuses.TryGetValue(type, out StatBonus mobBonus))
            bonuses.Add(mobBonus);
        if (abilityBonusProperties != null && abilityBonusProperties.TryGetValue(type, out StatBonus abilityBonus))
            bonuses.Add(abilityBonus);
        if (temporaryBonuses.TryGetValue(type, out StatBonus temporaryBonus))
            bonuses.Add(temporaryBonus);

        if (bonuses.Count == 0)
        {
            return;
        }

        foreach (StatBonus bonus in bonuses)
        {
            if (bonus.HasFixedModifier)
            {
                resultBonus.AddBonus(ModifyType.FIXED_TO, bonus.FixedModifier);
                return;
            }
            resultBonus.AddBonus(ModifyType.FLAT_ADDITION, bonus.FlatModifier);
            resultBonus.AddBonus(ModifyType.ADDITIVE, bonus.AdditiveModifier);
            resultBonus.AddBonus(ModifyType.MULTIPLY, (bonus.CurrentMultiplier - 1f) * 100f);
        }

        return;
    }

    public void SetMobBonuses(Dictionary<BonusType, StatBonus> dict)
    {
        mobBonuses = dict;
        UpdateActorData();
    }

    public override int GetResistance(ElementType element)
    {
        return ElementData.GetUncapResistance(element);
    }

    protected override HashSet<GroupType> GetGroupTypes()
    {
        HashSet<GroupType> types = new HashSet<GroupType>() { GroupType.NO_GROUP };

        if (CurrentActor != null)
        {
            types.UnionWith(CurrentActor.GetActorTags());
        }

        return types;
    }

    public override HashSet<BonusType> BonusesIntersection(IEnumerable<BonusType> abilityBonuses, IEnumerable<BonusType> bonuses)
    {
        HashSet<BonusType> actorBonuses = new HashSet<BonusType>();
        actorBonuses.UnionWith(statBonuses.Keys);
        actorBonuses.UnionWith(temporaryBonuses.Keys);
        actorBonuses.UnionWith(mobBonuses.Keys);
        if (abilityBonuses != null)
            actorBonuses.UnionWith(abilityBonuses);
        actorBonuses.IntersectWith(bonuses);
        return actorBonuses;
    }
}