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
    private void Update()
    {
        Move();
        timeToLive -= Time.deltaTime;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        int damage = 0;
        foreach (int d in projectileDamage.Values)
            damage += d;
        collision.gameObject.GetComponent<Actor>().ApplyDamage(damage);
        GameManager.Instance.ProjectilePool.ReturnToPool(this);
    }

    public void Move()
    {
        float dt = Time.deltaTime;
        //this.transform.position += currentHeading.normalized * currentSpeed * dt;
        this.transform.Translate(currentHeading.normalized * currentSpeed * dt);
        return;
    }
}