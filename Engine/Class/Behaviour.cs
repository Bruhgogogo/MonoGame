using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public class Behaviour : MonoObject
    {
        public bool Started;

        public virtual void Update(GameTime gameTime) { }
        public virtual void FixedUpdate(GameTime gameTime) { }
        public virtual void Start() { }
    }
}
