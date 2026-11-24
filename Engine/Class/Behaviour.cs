using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoEngine.Class
{
    public class Behaviour : Component
    {
        public bool Started = false;

        public Behaviour()
        {
            MonoEngine.BehaviourList.Add(this);
        }

        public virtual void Start() { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void FixedUpdate(GameTime gameTime) { }
        public virtual void OnCollision(CollisionResult2D collisionResult, GameObject collisionGameObject) { }
    }
}