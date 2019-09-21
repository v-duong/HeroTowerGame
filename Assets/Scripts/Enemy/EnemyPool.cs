using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : QueueObjectPool<EnemyActor>
{

    public EnemyPool(EnemyActor prefab) : base(prefab, 25)
    {
    }

    public EnemyActor GetEnemy(Transform inherit)
    {
        EnemyActor ret = Get();
        ret.Data.ClearData();
        ret.transform.position = inherit.transform.position;
        return ret;
    }

    public override void ReturnToPool(EnemyActor item)
    {
        Return(item);
    }

    public void ReturnAll()
    {
        ReturnAllActive();
    }
}
