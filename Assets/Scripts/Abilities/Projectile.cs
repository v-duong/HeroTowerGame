using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public AbilityBase abilityBase;
    public int pierceCount = 0;
    public float currentSpeed;
    public float timeToLive = 2.5f;
    public Func<Actor, Actor, Dictionary<ElementType, float>> damageCalculationCallback;
    public Vector3 currentHeading;
    public LinkedActorAbility linkedAbility;
    public AbilityOnHitDataContainer onHitData;
    public float inheritedDamagePercent;
    public bool isOffscreen = false;

    private ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
    private float particleWaitTime = 0;
    private List<Actor> targetsHit = new List<Actor>();

    //public List<AbilityEffect> attachedEffects;

    public Projectile()
    {
    }

    private void OnBecameInvisible()
    {
        isOffscreen = true;
    }

    private void OnBecameVisible()
    {
        isOffscreen = false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        timeToLive -= Time.fixedDeltaTime;
        if (timeToLive <= 0)
        {
            ReturnToPool();
        }

        if (isOffscreen)
        {
            Bounds bounds = StageManager.Instance.stageBounds;
            if (transform.position.x < bounds.center.x - bounds.extents.x || transform.position.x > bounds.center.x + bounds.extents.x ||
                transform.position.y < bounds.center.y - bounds.extents.y || transform.position.y > bounds.center.y + bounds.extents.y)
            {
                ReturnToPool();
            }
        }

        emitParams.position = transform.position;
        particleWaitTime -= Time.fixedDeltaTime;
        if (particleWaitTime <= 0)
            particleWaitTime = ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, transform.localScale.x);
        Move();
        //float angle = Vector3.Angle(transform.position, transform.position + currentHeading);
        //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.up = (transform.position + currentHeading * currentSpeed) - transform.position;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActiveAndEnabled)
            return;

        Actor actor = collision.gameObject.GetComponent<Actor>();
        Vector3 targetPosition;

        if (actor != null && !targetsHit.Contains(actor))
        {
            targetPosition = actor.transform.position;
            Dictionary<ElementType, float> projectileDamage = damageCalculationCallback.Invoke(actor, onHitData.sourceActor);
            actor.ApplyDamage(projectileDamage, onHitData, true);
            if (linkedAbility != null)
            {
                if (linkedAbility.LinkedAbilityData.type == AbilityLinkType.ON_EVERY_HIT ||
                    (linkedAbility.LinkedAbilityData.type == AbilityLinkType.ON_FIRST_HIT && targetsHit.Count == 0))
                {
                    linkedAbility.Fire(transform.position, targetPosition);
                }
            }
            targetsHit.Add(actor);
            pierceCount--;

            if (pierceCount < 0)
            {
                if (linkedAbility != null && linkedAbility.LinkedAbilityData.type == AbilityLinkType.ON_FINAL_HIT)
                {
                    linkedAbility.Fire(transform.position, targetPosition);
                }
                targetsHit.Clear();
                ReturnToPool();
            }
        }
        else
        {
            return;
        }
    }

    public void Move()
    {
        float dt = Time.fixedDeltaTime;
        //this.transform.position += currentHeading.normalized * currentSpeed * dt;
        transform.Translate(currentHeading.normalized * currentSpeed * dt, Space.World);
        return;
    }

    public void ReturnToPool()
    {
        damageCalculationCallback = null;
        linkedAbility = null;
        onHitData = null;
        GameManager.Instance.ProjectilePool.ReturnToPool(this);
    }
}