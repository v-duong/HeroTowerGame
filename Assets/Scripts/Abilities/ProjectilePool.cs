using UnityEngine;

public class ProjectilePool : QueueObjectPool<Projectile>
{
    public ProjectilePool(Projectile prefab) : base(prefab, 40)
    {
    }

    public Projectile GetProjectile()
    {
        Projectile p = Get();
        if (p.particles != null)
        {
            Component.Destroy(p.particles.gameObject);
            p.particles = null;
        }
        p.isOffscreen = false;
        return p;
    }

    public override void ReturnToPool(Projectile p)
    {
        if (p.particles != null)
        {
            p.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Component.Destroy(p.particles.gameObject);
            p.particles = null;
        }
        Return(p);
    }

    public void ReturnAll()
    {
        ReturnAllActive();
    }
}