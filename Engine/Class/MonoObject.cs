using System;

namespace MonoEngine.Class
{
    public class MonoObject
    {
        public bool Destroyed = false;

        public void Destroy()
        {
            if (Destroyed) return;
            Destroyed = true;
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