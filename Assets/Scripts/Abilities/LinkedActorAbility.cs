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
        if (linkedAbilityData.inheritsDamage)
        {
            float damageModifier = linkedAbilityData.inheritDamagePercent + linkedAbilityData.inheritDamagePercentScaling * abilityLevel;
            UpdateAbilityDamage(data, parentDamageLevels, damageModifier);
        }
        else
            UpdateAbilityDamage(data, abilityBase.damageLevels);

        UpdateAbility_AbilityType(data);
        UpdateAbility_ShotType(data);
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