﻿using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool canHitAllies = false;
    public bool canHitEnemies = false;
    public float currentSpeed;
    public float timeToLive = 2.5f;
    public Dictionary<ElementType, int> projectileDamage;
    public Vector3 currentHeading;
    public LinkedActorAbility linkedAbility;
    public float inheritedDamagePercent;

    //public List<AbilityEffect> attachedEffects;

    public Projectile()
    {
        projectileDamage = new Dictionary<ElementType, int>();
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
        //Debug.Log(collision.gameObject);
        Actor actor = collision.gameObject.GetComponent<Actor>();
        Vector3 targetPosition;
        if (actor != null)
        {
            targetPosition = actor.transform.position;
            int damage = 0;
            foreach (int d in projectileDamage.Values)
                damage += d;
            actor.ApplyDamage(damage);
        } else
        {
            return;
        }

        var _collider = GetComponent<Collider2D>();
        _collider.enabled = false;
        if (linkedAbility != null)
        {
            Dictionary<ElementType, int> newDamage = new Dictionary<ElementType, int>();
            foreach(KeyValuePair<ElementType, int> damage in projectileDamage)
            {

            }
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
        GameManager.Instance.ProjectilePool.ReturnToPool(this);
    }
}