using System.Collections.Generic;
using UnityEngine;

namespace PrismPanic.Utilities
{
    /// <summary>
    /// Generic object pool. Never Instantiate/Destroy pooled types in hot paths.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public ObjectPool(T prefab, Transform parent, int initialSize)
        {
            _prefab = prefab;
            _parent = parent;
            PreWarm(initialSize);
        }

        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T obj = Object.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                // Pool exhausted — grow by one
                obj = Object.Instantiate(_prefab, _parent);
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        public void ReturnAll(List<T> activeObjects)
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                Return(activeObjects[i]);
            }
            activeObjects.Clear();
        }

        public int CountInactive => _pool.Count;
    }
}
