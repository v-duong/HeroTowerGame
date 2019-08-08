using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool canHitAllies = false;
    public bool canHitEnemies = false;
    public float currentSpeed;
    public float timeToLive = 2.5f;
    public Dictionary<ElementType, double> projectileDamage;
    public Vector3 currentHeading;
    public LinkedActorAbility linkedAbility;
    public AbilityStatusEffectDataContainer statusData;
    public float inheritedDamagePercent;

    //public List<AbilityEffect> attachedEffects;

    public Projectile()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (timeToLive <= 0)
        {
            ReturnToPool();
        }
        Move();
        timeToLive -= Time.fixedDeltaTime;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.isActiveAndEnabled)
            return;
        Actor actor = collision.gameObject.GetComponent<Actor>();
        Vector3 targetPosition;
        if (actor != null)
        {
            targetPosition = actor.transform.position;
            actor.ApplyDamage(projectileDamage, statusData);
        } else
        {
            return;
        }

        var collider = GetComponent<Collider2D>();
        collider.enabled = false;
        if (linkedAbility != null)
        {
            linkedAbility.Fire(this.transform.position, targetPosition);
        }
        ReturnToPool();
    }

    public void Move()
    {
        float dt = Time.fixedDeltaTime;
        //this.transform.position += currentHeading.normalized * currentSpeed * dt;
        this.transform.Translate(currentHeading.normalized * currentSpeed * dt);
        return;
    }

    public void ReturnToPool()
    {
        projectileDamage = null;
        linkedAbility = null;
        statusData = null;
        GameManager.Instance.ProjectilePool.ReturnToPool(this);
    }
}