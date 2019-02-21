using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour {
    public int Id { get; }
    public int MinimumHealth { get; protected set; } //for cases of invincible/phased actors
    [SerializeField]
    public int MaximumHealth {get; protected set; }
    [SerializeField]
    public float CurrentHealth {get; protected set; }

    public bool HealthIsHitsToKill { get; protected set; } //health is number of hits to kill

    public int MaximumShield { get; protected set; }
    public float CurrentShield { get; protected set; }
    public int BaseShield { get; protected set; }
    public int BaseArmor { get; protected set; }
    public int BaseDodgeRating { get; protected set; }
    public int BaseAttackPhasing { get; protected set; }
    public int BaseMagicPhasing { get; protected set; }
    public int BaseResolveRating { get; protected set; }

    public Dictionary<ElementType, int> BaseResistances { get; protected set; }
    public float movementSpeed;
    public float actorTimeScale = 1f;

    protected UIHealthBar healthBar;
    protected List<ActorAbility> abilitiesList;

    public List<Actor> targetList;

    public virtual void Awake()
    {
        if (abilitiesList == null)
            abilitiesList = new List<ActorAbility>();
        if (targetList == null)
            targetList = new List<Actor>();
        CurrentHealth = MaximumHealth;
    }

    public virtual void Start()
    {

    }
    
    public void InitializeHealthBar()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        healthBar.Initialize(this.MaximumHealth, this.CurrentHealth, this.transform);
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }

    public void ModifyCurrentHealth(int mod)
    {
        if (CurrentHealth - mod > MaximumHealth)
            CurrentHealth = MaximumHealth;
        else
            CurrentHealth -= mod;
        if (CurrentHealth <= 0)
        {
            Death();
        }
    }

    public virtual void Death()
    {

    }

    public void Shoot(Actor target, AbilityBase ability)
    {

    }

    public void ApplyDamage(int damage)
    {
        ModifyCurrentHealth(damage);
        healthBar.UpdateHealthBar(MaximumHealth, CurrentHealth);
    }


    public bool IsAlive
    {
        get { return GetCurrentHealth() > 0.0f;  }
    }

    public bool IsDead
    {
        get { return GetCurrentHealth() <= 0.0f; }
    }

}