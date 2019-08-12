using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public string abilityId;
    public bool canHitAllies = false;
    public bool canHitEnemies = false;
    public float currentSpeed;
    public float timeToLive = 2.5f;
    public Dictionary<ElementType, double> projectileDamage;
    public Vector3 currentHeading;
    public LinkedActorAbility linkedAbility;
    public AbilityOnHitDataContainer statusData;
    public float inheritedDamagePercent;
    private ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
    private float particleWaitTime = 0;

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
        emitParams.position = transform.position;
        particleWaitTime -= Time.fixedDeltaTime;
        if (particleWaitTime <= 0)
            particleWaitTime = ParticleManager.Instance.EmitAbilityParticle(abilityId, emitParams, this.transform.localScale.x);
        Move();
        //float angle = Vector3.Angle(transform.position, transform.position + currentHeading);
        //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.up = (transform.position + currentHeading * currentSpeed) - transform.position;
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
        }
        else
        {
            return;
        }

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
        this.transform.Translate(currentHeading.normalized * currentSpeed * dt, Space.World);
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