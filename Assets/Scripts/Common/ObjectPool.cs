using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPool<T> where T : Component
{
    private readonly T prefab;

    private readonly Queue<T> pool = new Queue<T>();

    protected ObjectPool(T prefab, int preWarm = 0)
    {
        this.prefab = prefab;
        if (preWarm > 0)
            for (int i = 0; i < preWarm; i++)
            {
                var item = GameObject.Instantiate(prefab);
                item.gameObject.SetActive(false);
                pool.Enqueue(item);
            }
    }

    protected T Get()
    {
        T item;

        if (pool.Count == 0)
            item = GameObject.Instantiate(prefab);
        else
            item = pool.Dequeue();
        item.gameObject.SetActive(true);

        return item;
    }

    protected void Return(T item)
    {
        item.gameObject.SetActive(false);
        pool.Enqueue(item);
    }

    public abstract void ReturnToPool(T item);

}