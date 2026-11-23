using System;
using System.Collections.Generic;

namespace MonoEngine.MonoUtils
{
    public abstract class ObjectPool<T>
    {
        private readonly Stack<T> pool = new Stack<T>();
        private readonly int maxCapacity;

        protected ObjectPool(int maxCapacity = int.MaxValue)
        {
            this.maxCapacity = maxCapacity;
        }

        protected abstract T OnCreate();
        protected abstract void OnGet(T obj);
        protected abstract void OnRelease(T obj);
        protected abstract void OnDestroy(T obj);

        public T Get()
        {
            T item;
            if (pool.Count > 0)
            {
                item = pool.Pop();
            }
            else
            {
                item = OnCreate();
            }
            OnGet(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;

            if (pool.Count < maxCapacity)
            {
                OnRelease(item);
                pool.Push(item);
            }
            else
            {
                OnDestroy(item);
            }
        }

        public void Preload(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = OnCreate();
                OnRelease(item);
                pool.Push(item);
            }
        }

        public void Clear()
        {
            foreach (T item in pool)
            {
                OnDestroy(item);
            }
            pool.Clear();
        }

        public int CountInactive => pool.Count;
    }
}