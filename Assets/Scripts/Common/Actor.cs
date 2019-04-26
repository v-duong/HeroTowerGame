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

    public void InitializeHealthBar()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, this.transform);
    }


    public abstract void Death();

    public void Shoot(ActorData target, AbilityBase ability)
    {
    }

    public void AddAbilityToList(ActorAbility ability)
    {
        instancedAbilitiesList.Add(ability);
        GameObject newObject = Instantiate(ResourceManager.Instance.AbilityContainerPrefab, transform);
        AbilityColliderContainer abilityContainer = newObject.GetComponent<AbilityColliderContainer>();
        abilityContainer.ability = ability;
        abilityContainer.parentActor = this;
        abilityContainer.transform.position = transform.position;

        var collider = newObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.abilityBase.targetRange;
        ability.abilityCollider = collider;
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
        if (Data.CurrentHealth <= 0)
        {
            Death();
        }
    }


    public void ApplyDamage(int damage)
    {
        ModifyCurrentHealth(damage);
        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth);
    }
}
