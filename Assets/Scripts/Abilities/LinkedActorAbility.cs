using System.Collections.Generic;
using UnityEngine;

public class LinkedActorAbility : ActorAbility
{
    private readonly Dictionary<ElementType, AbilityDamageBase> parentDamageLevels;
    private LinkedAbilityData linkedAbilityData;

    public LinkedActorAbility(AbilityBase ability, int layer) : base(ability, layer)
    {
    }

    public LinkedActorAbility(LinkedAbilityData linkedAbility, int layer, Dictionary<ElementType, AbilityDamageBase> damageLevels)
        : this(ResourceManager.Instance.GetAbilityBase(linkedAbility.abilityId), layer)
    {
        this.linkedAbilityData = linkedAbility;
        this.parentDamageLevels = damageLevels;
    }

    public LinkedActorAbility(LinkedAbilityData linkedAbility, int layer)
    : this(ResourceManager.Instance.GetAbilityBase(linkedAbility.abilityId), layer)
    {
        this.linkedAbilityData = linkedAbility;
    }

    public override void UpdateAbilityStats(HeroData data)
    {
        UpdateAbilityBonusProperties();
        if (linkedAbilityData.inheritsDamage)
        {
            float damageModifier = linkedAbilityData.inheritDamagePercent + linkedAbilityData.inheritDamagePercentScaling * abilityLevel;
            UpdateAbility_Damage(data, parentDamageLevels, damageModifier);
        }
        else
            UpdateAbility_Damage(data, abilityBase.damageLevels);

        UpdateAbility_AbilityType(data);
        UpdateAbility_ShotType(data);
        UpdateAbility_StatusBonuses(data);
    }

    public void Fire(Vector3 origin, Vector3 target)
    {
        switch (abilityBase.abilityShotType)
        {
            case AbilityShotType.PROJECTILE:
                FireProjectile(origin, target);
                break;

            case AbilityShotType.ARC_AOE:
                FireArcAoe(origin, target);
                break;

            case AbilityShotType.RADIAL_AOE:
            case AbilityShotType.NOVA_AOE:
                FireRadialAoe(origin, target);
                break;
        }
    }
}