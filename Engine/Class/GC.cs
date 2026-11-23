using System;

namespace MonoEngine.Class
{
    public sealed class GCObject
    {
        private MonoObject objectToGC;
        private DateTime timeStamp;
        private float lifeTime { get; }
        

        public GCObject(MonoObject monoObject, float lifeTime)
        {
            objectToGC = monoObject;
            timeStamp = DateTime.Now;
            this.lifeTime = lifeTime;
        }

        public bool ShouldDestroy()
        {
            return (DateTime.Now - timeStamp).TotalSeconds >= lifeTime;
        }

        public bool IsObjectDestroyed()
        {
            return objectToGC.Destroyed;
        }

        public void Destroy()
        {
            objectToGC.Destroy();
        }
    }
}
