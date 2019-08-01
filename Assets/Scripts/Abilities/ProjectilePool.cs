using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : ObjectPool<Projectile>
{

    public ProjectilePool(Projectile prefab) : base(prefab, 40)
    { 
    }


    public Projectile GetProjectile()
    {
        Projectile p = Get();
        return p;
    }

    public override void ReturnToPool(Projectile p)
    {
        Return(p);
    }

    public void ReturnAll()
    {
        ReturnAllActive();
    }
}
