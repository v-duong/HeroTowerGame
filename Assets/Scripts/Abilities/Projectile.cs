using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool canHitAllies = false;
    public bool canHitEnemies = false;
    public float currentSpeed;
    public float timeToLive = 1.5f;
    public Dictionary<ElementType, int> projectileDamage;
    public Vector3 currentHeading;

    //public List<AbilityEffect> attachedEffects;

    public Projectile()
    {
        projectileDamage = new Dictionary<ElementType, int>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Move();
        timeToLive -= Time.fixedDeltaTime;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!this.isActiveAndEnabled)
            return;
        int damage = 0;
        foreach (int d in projectileDamage.Values)
            damage += d;
        collision.gameObject.GetComponent<Actor>().ApplyDamage(damage);
        
        var _collider = GetComponent<Collider2D>();
        _collider.enabled = false;

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