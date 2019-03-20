using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : ObjectPool<EnemyActor>
{

    public EnemyPool(EnemyActor prefab) : base(prefab, 10)
    {
    }

    public EnemyActor GetEnemy()
    {
        return Get();
    }

    public override void ReturnToPool(EnemyActor item)
    {
        Return(item);
    }
}
