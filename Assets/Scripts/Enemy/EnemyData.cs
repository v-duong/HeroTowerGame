using System;

public class EnemyData : ActorData
{
    protected EnemyBase enemyBaseData;

    public EnemyData() : base()
    {
        Id = Guid.NewGuid();
    }

    public void SetBase(EnemyBase enemyBase)
    {
        enemyBaseData = enemyBase;
        MaximumHealth = (int)(enemyBase.level * enemyBase.healthScaling + 150);
        CurrentHealth = MaximumHealth;
        movementSpeed = enemyBase.movementSpeed;
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            ElementType element = (ElementType)i;
            Resistances[element] = enemyBase.resistances[i];
        }
    }

    public override void UpdateActorData()
    {
        movementSpeed = (float)CalculateActorStat(BonusType.MOVEMENT_SPEED, enemyBaseData.movementSpeed);
    }

    public override void GetTotalStatBonus(BonusType type, StatBonus bonus)
    {
        StatBonus resultBonus;
        if (bonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = bonus;
        bool hasStatBonus = false, hasTemporaryBonus = false;

        if (statBonuses.TryGetValue(type, out StatBonus statBonus))
            hasStatBonus = true;
        if (temporaryBonuses.TryGetValue(type, out StatBonus temporaryBonus))
            hasTemporaryBonus = true;

        if (!hasStatBonus && !hasTemporaryBonus)
        {
            return;
        }

        else if (hasStatBonus && statBonus.hasSetModifier)
        {
            resultBonus.hasSetModifier = true;
            resultBonus.setModifier = statBonus.setModifier;
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
        return;
    }
}