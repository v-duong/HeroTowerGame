﻿using System;
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

    public void SetBase(EnemyBase enemyBase, RarityType rarity, int level)
    {
        BaseData = enemyBase;
        Name = enemyBase.idName;
        BaseHealth = (float)(Helpers.GetEnemyHealthScaling(level) * enemyBase.healthScaling);
        MaximumHealth = (int)BaseHealth;
        CurrentHealth = MaximumHealth;
        movementSpeed = enemyBase.movementSpeed;
        minAttackDamage = (int)(enemyBase.attackDamageMinMultiplier * Helpers.GetEnemyDamageScaling(level));
        maxAttackDamage = (int)(enemyBase.attackDamageMaxMultiplier * Helpers.GetEnemyDamageScaling(level));
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            ElementType element = (ElementType)i;
            ElementData[element] = enemyBase.resistances[i];
        }
    }

    public override void UpdateActorData()
    {
        ApplyHealthBonuses();
        ApplySoulPointBonuses();

        MaximumManaShield = CalculateActorStat(BonusType.GLOBAL_MAX_SHIELD, BaseManaShield);

        if (MaximumManaShield != 0)
        {
            float shieldPercent = CurrentManaShield / MaximumManaShield;
            CurrentManaShield = MaximumManaShield * shieldPercent;
        }
        movementSpeed = CalculateActorStat(BonusType.MOVEMENT_SPEED, BaseData.movementSpeed);

        Armor = CalculateActorStat(BonusType.GLOBAL_ARMOR, BaseArmor);
        DodgeRating = CalculateActorStat(BonusType.GLOBAL_DODGE_RATING, BaseDodgeRating);
        ResolveRating = CalculateActorStat(BonusType.GLOBAL_RESOLVE_RATING, BaseResolveRating);
        AttackPhasing = CalculateActorStat(BonusType.ATTACK_PHASING, BaseAttackPhasing);
        MagicPhasing = CalculateActorStat(BonusType.MAGIC_PHASING, BaseMagicPhasing);

        foreach(ActorAbility ability in abilities)
        {
            ability.UpdateAbilityStats(this);
        }

        base.UpdateActorData();
    }

    public override void GetTotalStatBonus(BonusType type, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus inputBonus)
    {
        StatBonus resultBonus;
        if (inputBonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = inputBonus;

        List<StatBonus> bonuses = new List<StatBonus>();

        if (statBonuses.TryGetValue(type, out StatBonus statBonus))
            bonuses.Add(statBonus);
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
}