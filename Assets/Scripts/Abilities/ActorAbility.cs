using System;
using System.Collections;
using UnityEngine;


[Serializable]
public class ActorAbility {
    public int abilityLevel;
    public AbilityBase abilityBase;
    public Collider2D collider;
    private bool firingRoutineRunning;
    private IEnumerator firingRoutine;
    private ContactFilter2D contactFilter;

    public void InitializeActorAbility()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        contactFilter.useLayerMask = true;
    }

    public int CalculateAbilityDamage()
    {
        return 0;
    }

    public bool StartFiring(Actor parent)
    {
        if (firingRoutineRunning)
            return false;
        else
        {
            firingRoutine = FiringRoutine();
            parent.StartCoroutine(firingRoutine);
            firingRoutineRunning = true;
            return true;
        }
    }

    public bool StopFiring(Actor parent)
    {
        if (firingRoutineRunning)
        {
            parent.StopCoroutine(firingRoutine);
            firingRoutineRunning = false;
            return true;
        }
        else
            return false;
    }

    private IEnumerator FiringRoutine()
    {
        bool fired = false;
        Collider2D[] results = new Collider2D[10];
        while (true)
        {
            if (collider.OverlapCollider(contactFilter, results) != 0)
            {
                var p = GameManager.Instance.ProjectilePool.GetProjectile();
                p.transform.position = this.collider.transform.position;
                p.currentHeading = results[0].transform.position - this.collider.transform.position;
                p.currentHeading.z = 0;
                p.currentSpeed = abilityBase.projectileSpeed;
                
                p.projectileDamage = CalculateAbilityDamage();
                fired = true;
            }
            if (fired)
            {
                fired = false;
                yield return new WaitForSeconds(abilityBase.cooldown);
            } else
            {
                yield return null;
            }
        }
        
    }

}

