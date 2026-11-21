using MonoEngine.Exceptions;

using System;

namespace MonoEngine.Class
{
    public class MonoObject
    {
        public bool Destroyed = false;

        public event Action<MonoObject> OnDestroySignal;

        public MonoObject()
        {
            MonoEngine.ObjectList.Add(this);
        }

        public void Destroy()
        {
            if (Destroyed) throw new DoubleDestroyException();
            Destroyed = true;
            OnDestroySignal?.Invoke(this);
            OnDestroy();
        }

        public virtual void OnDestroy()
        {

        }

        public static implicit operator bool(MonoObject obj)
        {
            return obj != null && !obj.Destroyed;
        }
    }
}