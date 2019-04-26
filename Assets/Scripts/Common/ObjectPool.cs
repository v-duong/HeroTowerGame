using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;


public abstract class ObjectPool<T> where T : Component
{
    private readonly T prefab;

    private readonly Queue<T> pool = new Queue<T>();
    private readonly List<T> inUseObjects = new List<T>();

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
        inUseObjects.Add(item);
        return item;
    }

    protected void Return(T item)
    {
        inUseObjects.Remove(item);
        item.gameObject.SetActive(false);
        pool.Enqueue(item);
    }

    protected void ReturnAllActive()
    {
        foreach(T item in inUseObjects)
        {
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }
        inUseObjects.Clear();
    }

    public abstract void ReturnToPool(T item);

}