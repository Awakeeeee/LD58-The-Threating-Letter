using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFactory<T> where T : MonoBehaviour, IPoolObject
{
    private Transform active_root;
    private Transform pool_root;
    private T prefab;
    private int initial_count;
    private Queue<T> mPool;
    private List<T> mActives;

    public ObjectFactory(T prefab, Transform active, Transform pool, int initials)
    {
        this.prefab = prefab;
        active_root = active;
        pool_root = pool;
        initial_count = initials;
        mPool = new Queue<T>();
        mActives = new List<T>();

        Initialize();
    }

    public T Get()
    {
        T obj = null;

        if (mPool.Count > 0)
        {
            obj = mPool.Dequeue();
        }
        else
        {
            obj = CreateObject();
        }

        if (obj != null)
        {
            obj.transform.SetParent(active_root);
            obj.gameObject.SetActive(true);
            mActives.Add(obj);
            obj.OnReuse();
        }

        return obj;
    }

    public List<T> GetAllActives()
    {
        return mActives;
    }

    public void Return(T obj)
    {
        if (obj == null || !mActives.Contains(obj))
        {
            return;
        }

        mActives.Remove(obj);
        obj.OnRecycle();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(pool_root);
        mPool.Enqueue(obj);
    }

    private T CreateObject()
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectFactory: Prefab is null!");
            return null;
        }

        T obj = Object.Instantiate(prefab, pool_root);
        obj.gameObject.SetActive(false);
        obj.OnCreate();

        return obj;
    }

    private void Initialize()
    {
        for (int i = 0; i < initial_count; i++)
        {
            T obj = CreateObject();
            if (obj != null)
            {
                mPool.Enqueue(obj);
            }
        }
    }

    public void ReturnAll()
    {
        var activesCopy = new List<T>(mActives);
        foreach (var obj in activesCopy)
        {
            Return(obj);
        }
    }

    public void Clear()
    {
        ReturnAll();

        while (mPool.Count > 0)
        {
            T obj = mPool.Dequeue();
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }

        mPool.Clear();
        mActives.Clear();
    }

    public int ActiveCount => mActives.Count;
    public int PoolCount => mPool.Count;
    public int TotalCount => ActiveCount + PoolCount;

    public void Log()
    {
        Debug.Log($"ObjectFactory<{typeof(T).Name}>: Active={ActiveCount}, Pool={PoolCount}, Total={TotalCount}");
    }
}

public interface IPoolObject
{
    void OnCreate();
    void OnRecycle();
    void OnReuse();
}
