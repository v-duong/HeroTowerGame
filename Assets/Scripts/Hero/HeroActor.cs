using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroActor : Actor
{


    // Use this for initialization
    public override void Start()
    {
        ActorAbility a = new ActorAbility(ResourceManager.Instance.GetAbilityBase("Fireball"));
        a.InitializeActorAbility();
    }

    // Update is called once per frame
    public void Update()
    {
        foreach (var x in instancedAbilitiesList)
        {
            x.StartFiring(this);
        }
    }

    public override void Death()
    {
        
    }
}
