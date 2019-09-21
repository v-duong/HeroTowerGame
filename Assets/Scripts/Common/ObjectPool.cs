using System.Collections.Generic;
using UnityEngine;

public abstract class QueueObjectPool<T> where T : Component
{
    private readonly T prefab;

    private readonly Queue<T> pool = new Queue<T>();
    private readonly List<T> inUseObjects = new List<T>();

    protected QueueObjectPool(T prefab, int preWarm = 0)
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

    protected void ReturnWithoutDeactivate(T item)
    {
        inUseObjects.Remove(item);
        pool.Enqueue(item);
    }

    protected void ReturnAllActive()
    {
        foreach (T item in inUseObjects)
        {
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }
        inUseObjects.Clear();
    }

    protected void DeactivatePooledObjects()
    {
        foreach (T item in pool)
        {
            item.gameObject.SetActive(false);
        }
    }

    public abstract void ReturnToPool(T item);
}

public abstract class StackObjectPool<T> where T : Component
{
    private readonly T prefab;

    private readonly Stack<T> pool = new Stack<T>();
    private readonly List<T> inUseObjects = new List<T>();

    protected StackObjectPool(T prefab, int preWarm = 0)
    {
        this.prefab = prefab;
        if (preWarm > 0)
            for (int i = 0; i < preWarm; i++)
            {
                var item = GameObject.Instantiate(prefab);
                item.gameObject.SetActive(false);
                pool.Push(item);
            }
    }

    protected T Get()
    {
        T item;

        if (pool.Count == 0)
            item = GameObject.Instantiate(prefab);
        else
            item = pool.Pop();
        item.gameObject.SetActive(true);
        inUseObjects.Add(item);
        return item;
    }

    protected void Return(T item)
    {
        inUseObjects.Remove(item);
        item.gameObject.SetActive(false);
        pool.Push(item);
    }

    protected void ReturnWithoutDeactivate(T item)
    {
        inUseObjects.Remove(item);
        pool.Push(item);
    }

    protected void ReturnAllActive()
    {
        foreach (T item in inUseObjects)
        {
            item.gameObject.SetActive(false);
            pool.Push(item);
        }
        inUseObjects.Clear();
    }

    protected void DeactivatePooledObjects()
    {
        foreach (T item in pool)
        {
            if (item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    public abstract void ReturnToPool(T item);
}