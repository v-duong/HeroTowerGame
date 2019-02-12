using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : PollingPool<Projectile>
{

    public ProjectilePool(Projectile prefab) : base(prefab, 50)
    { 
    }

    protected override bool IsActive(Projectile component)
    {
        return component.isActiveAndEnabled;
    }

    public Projectile GetProjectile()
    {
        return Get();
    }
}
