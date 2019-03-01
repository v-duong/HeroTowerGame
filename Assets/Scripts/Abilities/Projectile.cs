using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool canHitAllies = false;
    public bool canHitEnemies = false;
    public float currentSpeed;
    public float timeToLive = 1.5f;
    public int projectileDamage;
    public Vector3 currentHeading;

    public List<AbilityEffect> attachedEffects;

    // Update is called once per frame
    private void Update()
    {
        Move();
        timeToLive -= Time.deltaTime;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        collision.gameObject.GetComponent<Actor>().ApplyDamage(projectileDamage);
        this.gameObject.SetActive(false);
    }

    public void Move()
    {
        float dt = Time.deltaTime;
        //this.transform.position += currentHeading.normalized * currentSpeed * dt;
        this.transform.Translate(currentHeading.normalized * currentSpeed * dt);
        return;
    }
}