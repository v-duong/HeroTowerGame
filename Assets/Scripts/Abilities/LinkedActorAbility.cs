using System.Collections.Generic;
using UnityEngine;

public class LinkedActorAbility : ActorAbility
{
    private readonly Dictionary<ElementType, AbilityDamageBase> parentDamageLevels;
    public LinkedAbilityData LinkedAbilityData { get; private set; }

    public LinkedActorAbility(AbilityBase ability, int layer) : base(ability, layer)
    {
    }

    public LinkedActorAbility(LinkedAbilityData linkedAbility, int layer, Dictionary<ElementType, AbilityDamageBase> damageLevels)
        : this(ResourceManager.Instance.GetAbilityBase(linkedAbility.abilityId), layer)
    {
        this.LinkedAbilityData = linkedAbility;
        this.parentDamageLevels = damageLevels;
    }

    public LinkedActorAbility(LinkedAbilityData linkedAbility, int layer)
    : this(ResourceManager.Instance.GetAbilityBase(linkedAbility.abilityId), layer)
    {
        this.LinkedAbilityData = linkedAbility;
    }

    public override void UpdateAbilityStats(HeroData data)
    {
        UpdateAbilityBonusProperties();
        if (LinkedAbilityData.inheritsDamage)
        {
            float damageModifier = LinkedAbilityData.inheritDamagePercent + LinkedAbilityData.inheritDamagePercentScaling * abilityLevel;
            UpdateDamage(data, parentDamageLevels, damageModifier);
        }
        else
            UpdateDamage(data, abilityBase.damageLevels);

        UpdateTypeParameters(data);
        UpdateShotParameters(data);
        UpdateOnHitDataBonuses(data);
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