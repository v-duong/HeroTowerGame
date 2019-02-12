using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : PollingPool<EnemyActor>
{

    public EnemyPool(EnemyActor prefab) : base(prefab, 10)
    {
    }

    protected override bool IsActive(EnemyActor component)
    {
        return component.isActiveAndEnabled;
    }

    public EnemyActor GetEnemy()
    {
        return Get();
    }
}
