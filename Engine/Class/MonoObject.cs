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

        protected virtual void OnDestroy()
        {

        }

        public static implicit operator bool(MonoObject obj)
        {
            return obj != null && !obj.Destroyed;
        }

        public static void DestroyObject(MonoObject obj, float lifeTime)
        {
            GCObject gcObject = new GCObject(obj, lifeTime);
            MonoEngine.PendingGCList.Add(gcObject);
        }
    }
}