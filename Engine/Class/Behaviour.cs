using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public class Behaviour : Component
    {
        public bool Started;

        public Behaviour()
        {
            MonoEngine.BehaviourList.Add(this);
        }

        public virtual void Update(GameTime gameTime) { }
        public virtual void FixedUpdate(GameTime gameTime) { }
        public virtual void Start() { }
    }
}