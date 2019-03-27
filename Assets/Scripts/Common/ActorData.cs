using System.Collections.Generic;
using UnityEngine;

public abstract class ActorData
{
    public int Id { get; protected set; }
    public int Level { get; set; }
    public int Experience { get; set; }

    public float BaseHealth { get; protected set; }

    [SerializeField]
    public int MaximumHealth { get;  set; }

    [SerializeField]
    public float CurrentHealth { get;  set; }

    public int MinimumHealth { get; protected set; } //for cases of invincible/phased actors

    public bool HealthIsHitsToKill { get; protected set; } //health is number of hits to kill

    public float BaseSoulPoints { get; protected set; }
    public int MaximumSoulPoints { get;  set; }
    public float CurrentSoulPoints { get;  set; }

    public int BaseShield { get; protected set; }
    public int MaximumShield { get;  set; }
    public float CurrentShield { get;  set; }

    public int BaseArmor { get; protected set; }
    public int BaseDodgeRating { get; protected set; }
    public int BaseAttackPhasing { get; protected set; }
    public int BaseMagicPhasing { get; protected set; }
    public int BaseResolveRating { get; protected set; }

    public ElementResistances Resistances { get; protected set; }

    public float movementSpeed;


    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }



    public bool IsAlive
    {
        get { return GetCurrentHealth() > 0.0f; }
    }

    public bool IsDead
    {
        get { return GetCurrentHealth() <= 0.0f; }
    }
}