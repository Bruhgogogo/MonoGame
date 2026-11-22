using Microsoft.Xna.Framework.Graphics;

namespace MonoEngine.Class
{
    public abstract class Renderer : Component
    {
        protected SpriteBatch spriteBatch;

        public Renderer()
        {
            spriteBatch = MonoEngine.SpriteBatch;
            MonoEngine.RendererList.Add(this);
        }

        public abstract void Draw();
        public abstract bool IsVisible();
    }
}