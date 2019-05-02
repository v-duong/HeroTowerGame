using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    protected UIHealthBar healthBar;
    protected List<ActorAbility> instancedAbilitiesList = new List<ActorAbility>();
    protected List<AbilityColliderContainer> abilityColliders = new List<AbilityColliderContainer>();
    public abstract ActorType GetActorType();

    protected int nextMovementNode;

    public abstract void Death();

    public void InitializeHealthBar()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, this.transform);
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
        abilityContainer.collider = collider;
        collider.isTrigger = true;

        if (ability.TargetType == AbilityTargetType.ENEMY)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("EnemyDetect");
        }

    }

    public void ModifyCurrentHealth(int mod)
    {
        if (Data.CurrentHealth - mod > Data.MaximumHealth)
            Data.CurrentHealth = Data.MaximumHealth;
        else
            Data.CurrentHealth -= mod;

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth);
        if (Data.CurrentHealth <= 0)
        {
            Death();
        }
    }


    public void ApplyDamage(int damage)
    {
        ModifyCurrentHealth(damage);
    }
}


public enum ActorType
{
    ENEMY,
    ALLY
}