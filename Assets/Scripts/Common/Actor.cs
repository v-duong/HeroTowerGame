using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour {
    protected int minimumHealth; //for cases of invincible/phased actors
    [SerializeField]
    protected int maximumHealth;
    public float maximumHealthModifier = 1f;
    [SerializeField]
    protected float currentHealth;
    [SerializeField]
    protected int maximumMana;
    public float maximumManaModifier = 1f;
    [SerializeField]
    protected float currentMana;
    [SerializeField]

    public bool healthIsHitsToKill; //health is number of hits to kill

    protected int maximumShield;
    protected float currentShield;
    protected int baseArmor;
    protected int baseEvasion;
    protected float baseRegen;
    protected int baseDodge;
    protected int baseMagicDodge;

    protected int baseFireResistance;
    protected int baseColdResistance;
    protected int baseLightningResistance;
    protected int baseEarthResistance;
    protected int baseToxicResistance;
    protected int baseVoidResistance;

    protected int baseStrength;
    protected int baseIntelligence;
    protected int baseAgility;
    protected int baseWill;

    public float actorTimeScale = 1f;
    [SerializeField]
    protected UIHealthBar m_healthBar;
    [SerializeField]
    protected List<ActorAbility> m_abilitiesList;
    public List<Actor> targetList;

    public virtual void Awake()
    {
        if (m_abilitiesList == null)
            m_abilitiesList = new List<ActorAbility>();
        if (targetList == null)
            targetList = new List<Actor>();
        currentHealth = maximumHealth;
    }

    public virtual void Start()
    {

    }
    
    public void InitializeHealthBar()
    {
        m_healthBar = GetComponentInChildren<UIHealthBar>();
        m_healthBar.Initialize(this.maximumHealth, this.currentHealth, this.transform);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ModifyCurrentHealth(float mod)
    {
        if (currentHealth - mod > maximumHealth * maximumHealthModifier)
            currentHealth = maximumHealth * maximumManaModifier;
        else
            currentHealth -= mod;
        if (currentHealth <= 0)
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

    public void ApplyDamage(float damage)
    {
        ModifyCurrentHealth(damage);
        m_healthBar.UpdateHealthBar(maximumHealth, currentHealth);
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