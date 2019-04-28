using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroActor : Actor
{
    public new HeroData Data
    {
        get
        {
            return (HeroData)base.Data;
        }
        private set
        {
            base.Data = value;
        }
    }

    public void Initialize(HeroData data)
    {
        Data = data;
        if (data.GetAbilityFromSlot(0) != null)
        {
            ActorAbility firstAbility = data.GetAbilityFromSlot(0);
            this.AddAbilityToList(firstAbility);
        }
        if (data.GetAbilityFromSlot(1) != null)
        {
            ActorAbility secondAbility = data.GetAbilityFromSlot(1);
            this.AddAbilityToList(secondAbility);
        }

    }

    public void OnEnable()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StartFiring(this);
        }
    }

    private void OnDisable()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }
    }

    public override void Death()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }
    }

    public override ActorType GetActorType()
    {
        return ActorType.ALLY;
    }
}
