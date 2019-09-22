using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : QueueObjectPool<Projectile>
{

    public ProjectilePool(Projectile prefab) : base(prefab, 40)
    { 
    }


    public Projectile GetProjectile()
    {
        Projectile p = Get();
        p.isOffscreen = false;
        return p;
    }

    public override void ReturnToPool(Projectile p)
    {
        if (p.particles !=null)
        {
            p.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            GameObject.Destroy(p.particles);
            p.particles = null;
        }
        Return(p);
    }

    public void ReturnAll()
    {
        ReturnAllActive();
    }
}
