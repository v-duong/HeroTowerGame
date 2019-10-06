using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public AbilityBase abilityBase;
    public float homingRate = 0f;
    public Actor currentTarget = null;
    public int pierceCount = 0;
    public int chainCount = 0;
    public float currentSpeed;
    public float timeToLive = 2.5f;
    public List<Actor> sharedHitList;
    public Vector3 currentHeading { get; private set; }
    public LinkedActorAbility linkedAbility;
    public AbilityOnHitDataContainer onHitData;
    public float inheritedDamagePercent;
    public bool isOffscreen = false;
    public ParticleSystem particles;
    public int layerMask;
    private Collider2D[] hits = new Collider2D[12];

    //private ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
    //private float particleWaitTime = 0;
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

        /*
        emitParams.position = transform.position;

        particleWaitTime -= Time.fixedDeltaTime;
        if (particleWaitTime <= 0)
            particleWaitTime = ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, transform.localScale.x);
            */

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

        if (actor != null && !IsTargetAlreadyHit(actor) && !actor.Data.IsDead)
        {
            bool didProjHit = onHitData.sourceAbility.ApplyDamageToActor(actor, true);
            AddToAlreadyHit(actor);
            currentTarget = null;

            if (!didProjHit)
                return;

            if (linkedAbility != null)
            {
                if (linkedAbility.LinkedAbilityData.type == AbilityLinkType.ON_EVERY_HIT ||
                    (linkedAbility.LinkedAbilityData.type == AbilityLinkType.ON_FIRST_HIT && targetsHit.Count == 0))
                {
                    linkedAbility.Fire(transform.position, actor);
                }
            }

            pierceCount--;

            if (pierceCount < 0 && chainCount > 0)
            {
                FindNextChainTarget(actor);
                chainCount--;
            }
            else if (pierceCount < 0 && chainCount <= 0)
            {
                if (linkedAbility != null && linkedAbility.LinkedAbilityData.type == AbilityLinkType.ON_FINAL_HIT)
                {
                    linkedAbility.Fire(transform.position, actor);
                }
                ReturnToPool();
            }
        }
        else
        {
            return;
        }
    }

    private void FindNextChainTarget(Actor actor)
    {
        List<Actor> possibleTargets = new List<Actor>();
        Physics2D.OverlapCircleNonAlloc(actor.transform.position, 3, hits, layerMask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                break;
            Actor newTarget = hit.GetComponent<Actor>();
            if (IsTargetAlreadyHit(actor))
                continue;
            possibleTargets.Add(actor);
        }

        if (possibleTargets.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, possibleTargets.Count);
            if (possibleTargets[index] != null)
            {
                currentTarget = possibleTargets[index];
                currentHeading = (possibleTargets[index].transform.position - this.transform.position).normalized;
            }
        }
        else if (hits.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, hits.Length);
            if (hits[index] != null)
            {
                currentTarget = null;
                currentHeading = (hits[index].transform.position - this.transform.position).normalized;
            }
        }
    }

    private void AddToAlreadyHit(Actor target)
    {
        if (sharedHitList != null)
            sharedHitList.Add(target);
        targetsHit.Add(target);
    }

    private bool IsTargetAlreadyHit(Actor target)
    {
        if (sharedHitList != null && sharedHitList.Contains(target))
            return true;
        if (targetsHit.Contains(target))
            return true;

        return false;
    }

    public void Move()
    {
        float dt = Time.deltaTime;
        //this.transform.position += currentHeading.normalized * currentSpeed * dt;
        if (currentTarget != null)
        {
            Vector3 headingShift = Vector3.RotateTowards(currentHeading.normalized, (currentTarget.transform.position - transform.position).normalized, homingRate * Mathf.Deg2Rad * dt, 1);
            currentHeading = headingShift.normalized;
        }
        transform.Translate(currentHeading * currentSpeed * dt, Space.World);
        return;
    }

    public void SetHeading(Vector3 heading)
    {
        currentHeading = heading.normalized;
    }

    public void ReturnToPool()
    {
        linkedAbility = null;
        onHitData = null;
        targetsHit.Clear();
        sharedHitList = null;
        currentTarget = null;
        StageManager.Instance.BattleManager.ProjectilePool.ReturnToPool(this);
    }
}