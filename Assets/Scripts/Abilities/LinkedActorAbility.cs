using System;
using System.Collections.Generic;
using UnityEngine;

public class LinkedActorAbility : ActorAbility
{
    private readonly Dictionary<ElementType, AbilityDamageBase> parentDamageLevels;
    private LinkedAbilityData linkedAbilityData;

    public LinkedActorAbility(AbilityBase ability) : base(ability)
    {
    }

    public LinkedActorAbility(LinkedAbilityData linkedAbility, Dictionary<ElementType, AbilityDamageBase> damageLevels)
        : this(ResourceManager.Instance.GetAbilityBase(linkedAbility.abilityId))
    {
        this.linkedAbilityData = linkedAbility;
        this.parentDamageLevels = damageLevels;
    }

    public LinkedActorAbility(LinkedAbilityData linkedAbility)
    : this(ResourceManager.Instance.GetAbilityBase(linkedAbility.abilityId))
    {
        this.linkedAbilityData = linkedAbility;
    }

    public override void UpdateAbilityStats(HeroData data)
    {
        UpdateAbilityDamage(data);
    }

    protected override void UpdateAbilityDamage(HeroData data)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;
        var elementValues = Enum.GetValues(typeof(ElementType));

        Dictionary<ElementType, AbilityDamageBase> damageLevels;

        if (linkedAbilityData.inheritsDamage)
            damageLevels = parentDamageLevels;
        else
            damageLevels = abilityBase.damageLevels;

        foreach (ElementType e in elementValues)
        {
            MinMaxRange newDamageRange = new MinMaxRange();

            if (damageLevels.ContainsKey(e))
            {
                MinMaxRange abilityBaseDamage = damageLevels[e].damage[abilityLevel];
                newDamageRange.min = abilityBaseDamage.min;
                newDamageRange.max = abilityBaseDamage.max;
            }
            if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
                {
                    float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                    MinMaxRange weaponDamage = weapon.GetWeaponDamage(e);
                    newDamageRange.min = (int)(weaponDamage.min * weaponMulti);
                    newDamageRange.max = (int)(weaponDamage.max * weaponMulti);
                }
            }

            List<BonusType> min = new List<BonusType>();
            List<BonusType> max = new List<BonusType>();
            List<BonusType> multi = new List<BonusType>();
            minBonus = new StatBonus();
            maxBonus = new StatBonus();
            multiBonus = new StatBonus();

            Helpers.GetDamageTypes(e, abilityType, abilityBase.abilityShotType, abilityBase.groupTypes, min, max, multi);

            foreach (BonusType bonusType in min)
            {
                data.GetTotalStatBonus(bonusType, minBonus);
            }
            foreach (BonusType bonusType in max)
            {
                data.GetTotalStatBonus(bonusType, maxBonus);
            }
            foreach (BonusType bonusType in multi)
            {
                data.GetTotalStatBonus(bonusType, multiBonus);
            }

            newDamageRange.min = (int)(minBonus.CalculateStat(newDamageRange.min) * abilityBase.flatDamageMultiplier);
            newDamageRange.max = (int)(maxBonus.CalculateStat(newDamageRange.max) * abilityBase.flatDamageMultiplier);

            newDamageRange.min = multiBonus.CalculateStat(newDamageRange.min);
            newDamageRange.max = multiBonus.CalculateStat(newDamageRange.max);

            if (newDamageRange.IsZero())
                damageBase.Remove(e);
            else
            {
                damageBase[e] = newDamageRange;
            }
        }
    }

    public void Fire(Vector3 origin, Vector3 target)
    {
        switch (abilityBase.abilityShotType)
        {
            case AbilityShotType.PROJECTILE:
                FireProjectile(origin, target, CalculateDamageDict());
                break;

            case AbilityShotType.ARC_AOE:
                FireArcAoe(origin, target, CalculateDamageDict());
                break;

            case AbilityShotType.RADIAL_AOE:
            case AbilityShotType.NOVA_AOE:
                FireRadialAoe(origin, target, CalculateDamageDict());
                break;
        }
    }
}