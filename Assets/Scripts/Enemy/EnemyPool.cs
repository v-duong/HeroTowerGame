using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : QueueObjectPool<EnemyActor>
{

    public EnemyPool(EnemyActor prefab) : base(prefab, 60)
    {
    }

    public EnemyActor GetEnemy(Transform inherit)
    {
        EnemyActor ret = Get();
        ret.actorTimeScale = 1f;
        ret.attackLocks = 0;
        ret.actorTags.Clear();
        ret.ClearStatusEffects(false);
        ret.InitData();
        ret.transform.position = inherit.transform.position;
        ret.mobAffixes.Clear();
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

public class DamageTextPool : QueueObjectPool<FloatingDamageText>
{

    public DamageTextPool(FloatingDamageText prefab) : base(prefab, 60)
    {
    }

    public FloatingDamageText GetDamageText()
    {
        FloatingDamageText ret = Get();
        ret.ResetDuration();
        return ret;
    }

    public override void ReturnToPool(FloatingDamageText item)
    {
        Return(item);
    }

    public void ReturnAll()
    {
        ReturnAllActive();
    }
}
