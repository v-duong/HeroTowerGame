using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public Vector3 currentHeading;
    public float currentSpeed;
    public bool canHitEnemies = false;
    public bool canHitAllies = false;
    public float projectileDamage;
    public float timeToLive = 1.5f;

    public List<AbilityEffect> attachedEffects;

    // Update is called once per frame
    void Update () {
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
