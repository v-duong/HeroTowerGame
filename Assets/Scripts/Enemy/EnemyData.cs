using System;
using System.Collections.Generic;

public class EnemyData : ActorData
{
    public EnemyBase BaseData { get; protected set; }
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
        BaseData = enemyBase;
        Name = enemyBase.idName;
        CurrentActor = actor;
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
    }

    public override void UpdateActorData()
    {
        GroupTypes = GetGroupTypes();
        ApplyHealthBonuses();
        ApplySoulPointBonuses();

        MaximumManaShield = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_MAX_SHIELD).CalculateStat(BaseManaShield);

        if (MaximumManaShield != 0)
        {
            float shieldPercent = CurrentManaShield / MaximumManaShield;
            CurrentManaShield = MaximumManaShield * shieldPercent;
        }
        movementSpeed = GetMultiStatBonus(GroupTypes, BonusType.MOVEMENT_SPEED).CalculateStat(BaseData.movementSpeed);

        Armor = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_ARMOR).CalculateStat(BaseArmor);
        DodgeRating = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_DODGE_RATING).CalculateStat(BaseDodgeRating);
        ResolveRating = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_RESOLVE_RATING).CalculateStat(BaseResolveRating);
        AttackPhasing = GetMultiStatBonus(GroupTypes, BonusType.ATTACK_PHASING).CalculateStat(BaseAttackPhasing);
        MagicPhasing = GetMultiStatBonus(GroupTypes, BonusType.MAGIC_PHASING).CalculateStat(BaseMagicPhasing);

        AfflictedStatusDamageResistance = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_DAMAGE_RESISTANCE).CalculateStat(1f);
        AfflictedStatusThreshold = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_THRESHOLD).CalculateStat(1f);

        foreach (ActorAbility ability in abilities)
        {
            ability.UpdateAbilityStats(this);
        }

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