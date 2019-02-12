using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroActor : Actor {
	// Use this for initialization
	public override void Start () {
        ActorAbility a = new ActorAbility
        {
            abilityBase = ResourceManager.Instance.GetAbilityBase(0)
        };
        a.InitializeActorAbility();
        AddAbilityToList(a);
        ActorAbility b = new ActorAbility
        {
            abilityBase = ResourceManager.Instance.GetAbilityBase(1)
        };
        b.InitializeActorAbility();
        AddAbilityToList(b);
    }
	
	// Update is called once per frame
	public void Update () {
        foreach(var x in m_abilitiesList)
        {
            x.StartFiring(this);
        }
	}

    public override void Death()
    {
        return;
    }

    public void AddAbilityToList(ActorAbility ability)
    {
        m_abilitiesList.Add(ability);
        var collider = this.transform.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.abilityBase.baseTargetRange;
        ability.collider = collider;
        collider.isTrigger = true;
    }
}
