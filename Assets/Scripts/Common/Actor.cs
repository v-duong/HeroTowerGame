using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    protected UIHealthBar healthBar;
    protected List<ActorAbility> instancedAbilitiesList;
    public List<Actor> targetList;


    public virtual void Awake()
    {
        if (instancedAbilitiesList == null)
            instancedAbilitiesList = new List<ActorAbility>();
        if (targetList == null)
            targetList = new List<Actor>();
    }

    public virtual void Start()
    {
    }

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
        var collider = this.transform.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.abilityBase.targetRange;
        ability.collider = collider;
        collider.isTrigger = true;
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
