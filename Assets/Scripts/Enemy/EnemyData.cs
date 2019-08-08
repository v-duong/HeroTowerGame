using System;
using System.Collections.Generic;

public class EnemyData : ActorData
{
    public EnemyBase BaseData { get; protected set; }
    protected Dictionary<BonusType, StatBonus> mobBonuses;
    public int minAttackDamage;
    public int maxAttackDamage;

    public EnemyData() : base()
    {
        Id = Guid.NewGuid();
        mobBonuses = new Dictionary<BonusType, StatBonus>();
    }

    public void SetBase(EnemyBase enemyBase, RarityType rarity, int level)
    {
        BaseData = enemyBase;
        BaseHealth = (float)(Helpers.GetEnemyHealthScaling(level) * 15 * enemyBase.healthScaling);
        MaximumHealth = (int)BaseHealth;
        CurrentHealth = MaximumHealth;
        movementSpeed = enemyBase.movementSpeed;
        minAttackDamage = (int)(enemyBase.attackDamageMinMultiplier * Helpers.GetEnemyHealthScaling(level) * 5);
        maxAttackDamage = (int)(enemyBase.attackDamageMaxMultiplier * Helpers.GetEnemyHealthScaling(level) * 5);
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            ElementType element = (ElementType)i;
            Resistances[element] = enemyBase.resistances[i];
        }
    }

    public override void UpdateActorData()
    {
        float healthPercent = CurrentHealth / MaximumHealth;
        float shieldPercent = CurrentManaShield / MaximumManaShield;
        MaximumHealth = (int)CalculateActorStat(BonusType.MAX_HEALTH, BaseHealth);
        CurrentHealth = MaximumHealth * healthPercent;
        MaximumManaShield = (int)CalculateActorStat(BonusType.GLOBAL_MAX_SHIELD, BaseManaShield);
        CurrentManaShield = MaximumManaShield * shieldPercent;
        movementSpeed = (float)CalculateActorStat(BonusType.MOVEMENT_SPEED, BaseData.movementSpeed);
    }

    public override void GetTotalStatBonus(BonusType type, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus bonus)
    {
        StatBonus resultBonus;
        StatBonus abilityBonus = null;
        if (bonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = bonus;
        bool hasStatBonus = false, hasTemporaryBonus = false, hasMobBonus = false, hasAbilityBonus = false;

        if (statBonuses.TryGetValue(type, out StatBonus statBonus))
            hasStatBonus = true;
        if (temporaryBonuses.TryGetValue(type, out StatBonus temporaryBonus))
            hasTemporaryBonus = true;
        if (mobBonuses.TryGetValue(type, out StatBonus mobBonus))
            hasMobBonus = true;
        if (abilityBonusProperties != null && abilityBonusProperties.TryGetValue(type, out abilityBonus))
            hasAbilityBonus = true;

        if (!hasStatBonus && !hasTemporaryBonus && !hasMobBonus && !hasAbilityBonus)
        {
            return;
        }
        else if (hasStatBonus && statBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = statBonus.setModifier;
            return;
        }
        else if (hasMobBonus && mobBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = mobBonus.setModifier;
            return;
        }
        else if (hasAbilityBonus && abilityBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = abilityBonus.setModifier;
            return;
        }
        else if (hasTemporaryBonus && temporaryBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = temporaryBonus.setModifier;
            return;
        }

        if (hasStatBonus)
        {
            resultBonus.AddBonus(ModifyType.FLAT_ADDITION, statBonus.FlatModifier);
            resultBonus.AddBonus(ModifyType.ADDITIVE, statBonus.AdditiveModifier);
            resultBonus.AddBonus(ModifyType.MULTIPLY, (statBonus.CurrentMultiplier - 1) * 100);
        }
        if (hasTemporaryBonus)
        {
            resultBonus.AddBonus(ModifyType.FLAT_ADDITION, temporaryBonus.FlatModifier);
            resultBonus.AddBonus(ModifyType.ADDITIVE, temporaryBonus.AdditiveModifier);
            resultBonus.AddBonus(ModifyType.MULTIPLY, (temporaryBonus.CurrentMultiplier - 1) * 100);
        }
        if (hasMobBonus)
        {
            resultBonus.AddBonus(ModifyType.FLAT_ADDITION, mobBonus.FlatModifier);
            resultBonus.AddBonus(ModifyType.ADDITIVE, mobBonus.AdditiveModifier);
            resultBonus.AddBonus(ModifyType.MULTIPLY, (mobBonus.CurrentMultiplier - 1) * 100);
        }
        if (hasAbilityBonus)
        {
            resultBonus.AddBonus(ModifyType.FLAT_ADDITION, abilityBonus.FlatModifier);
            resultBonus.AddBonus(ModifyType.ADDITIVE, abilityBonus.AdditiveModifier);
            resultBonus.AddBonus(ModifyType.MULTIPLY, (abilityBonus.CurrentMultiplier - 1) * 100);
        }
        return;
    }

    public void SetMobBonuses(Dictionary<BonusType, StatBonus> dict)
    {
        mobBonuses = dict;
        UpdateActorData();
    }
}