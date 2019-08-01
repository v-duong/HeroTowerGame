﻿using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    private readonly List<ActorStatusEffect> statusEffects = new List<ActorStatusEffect>();
    protected UIHealthBar healthBar;
    protected List<ActorAbility> instancedAbilitiesList = new List<ActorAbility>();
    protected List<AbilityColliderContainer> abilityColliders = new List<AbilityColliderContainer>();
    protected int nextMovementNode;

    public abstract ActorType GetActorType();

    public abstract void Death();

    public void UpdateStatusEffects()
    {
        float dT = Time.deltaTime;
        int index = statusEffects.Count - 1;
        while (index >= 0)
        {
            statusEffects[index].Update(dT);
            index--;
        }
    }

    public void InitializeHealthBar()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, this.transform);
    }

    public void AddStatusEffect(ActorStatusEffect statusEffect)
    {
        statusEffects.Add(statusEffect);
    }

    public void RemoveStatusEffect(ActorStatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect);
    }

    public void AddAbilityToList(ActorAbility ability)
    {
        instancedAbilitiesList.Add(ability);

        GameObject newObject = Instantiate(ResourceManager.Instance.AbilityContainerPrefab, transform);
        AbilityColliderContainer abilityContainer = newObject.GetComponent<AbilityColliderContainer>();
        abilityContainer.ability = ability;
        abilityContainer.parentActor = this;
        abilityContainer.transform.position = transform.position;
        ability.abilityCollider = abilityContainer;

        var collider = newObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.abilityBase.targetRange;
        abilityContainer.abilityCollider = collider;
        collider.isTrigger = true;

        if (ability.TargetType == AbilityTargetType.ENEMY)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("EnemyDetect");
        }
    }

    public void ModifyCurrentHealth(double mod)
    {
        if (Data.CurrentHealth - mod > Data.MaximumHealth)
            Data.CurrentHealth = Data.MaximumHealth;
        else
            Data.CurrentHealth -= (float)mod;

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth);
        if (Data.CurrentHealth <= 0)
        {
            Death();
        }
    }

    public void ApplyDamage(Dictionary<ElementType, double> damage, AbilityStatusEffectDataContainer statusData, bool isHit = true)
    {
        double total = 0, physicalDamage, fireDamage, coldDamage, lightningDamage, earthDamage, divineDamage, voidDamage;

        if (damage.ContainsKey(ElementType.PHYSICAL))
        {
            physicalDamage = (1.0 - Data.Resistances[ElementType.PHYSICAL] / 100d) * damage[ElementType.PHYSICAL];
            total += physicalDamage;
        }

        if (damage.ContainsKey(ElementType.FIRE))
        {
            fireDamage = (1.0 - Data.Resistances[ElementType.FIRE] / 100d) * damage[ElementType.FIRE];
            total += fireDamage;
            if (Random.Range(0f, 100f) < statusData.burnChance)
            {
                Debug.Log(statusData.burnEffectiveness);
                AddStatusEffect(new BurningEffect(this, damage[ElementType.FIRE] * statusData.burnEffectiveness, 3));
            }
        }

        if (damage.ContainsKey(ElementType.COLD))
        {
            coldDamage = (1.0 - Data.Resistances[ElementType.COLD] / 100d) * damage[ElementType.COLD];
            total += coldDamage;
        }

        if (damage.ContainsKey(ElementType.LIGHTNING))
        {
            lightningDamage = (1.0 - Data.Resistances[ElementType.LIGHTNING] / 100d) * damage[ElementType.LIGHTNING];
            total += lightningDamage;
        }

        if (damage.ContainsKey(ElementType.EARTH))
        {
            earthDamage = (1.0 - Data.Resistances[ElementType.EARTH] / 100d) * damage[ElementType.EARTH];
            total += earthDamage;
        }

        if (damage.ContainsKey(ElementType.DIVINE))
        {
            divineDamage = (1.0 - Data.Resistances[ElementType.DIVINE] / 100d) * damage[ElementType.DIVINE];
            total += divineDamage;
        }

        if (damage.ContainsKey(ElementType.VOID))
        {
            voidDamage = (1.0 - Data.Resistances[ElementType.VOID] / 100d) * damage[ElementType.VOID];
            total += voidDamage;
        }

        ModifyCurrentHealth(total);
    }

    public void ApplySingleElementDamage(ElementType element, double damage, bool isHit = true)
    {
        double finalDamage = ((1.0 - Data.Resistances[element] / 100d) * damage);
        ModifyCurrentHealth(finalDamage);
    }
}

public enum ActorType
{
    ENEMY,
    ALLY
}