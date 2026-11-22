using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public sealed class RendererSprite : Renderer
    {
        public Texture2D Texture;
        public Vector2 Origin = Vector2.Zero;
        public Color TextureColor = Color.White;
        public int SortingLayer = 0;
        public bool FlipX = false, FlipY = false, Visible = true;

        private Transform transform;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
        }

        public override bool IsVisible()
        {
            return Visible && Texture != null && spriteBatch != null && TextureColor.A != 0;
        }

        public override void Draw()
        {
            Vector2 pixelSize = new Vector2(transform.Scale.X / Texture.Width, transform.Scale.Y / Texture.Height);
            Vector2 pixelOrigin = new Vector2(Origin.X * Texture.Width, Origin.Y * Texture.Height);
            spriteBatch.Draw(
                Texture,
                transform.Position,
                null,
                TextureColor,
                transform.Rotation,
                pixelOrigin,
                pixelSize,
                GetSpriteEffect(),
                SortingLayer
            );
        }

        private SpriteEffects GetSpriteEffect()
        {
            SpriteEffects flag = SpriteEffects.None;
            if (FlipX) flag |= SpriteEffects.FlipHorizontally;
            if (FlipY) flag |= SpriteEffects.FlipVertically;
            return flag;
        }
    }
}
