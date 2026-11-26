using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public sealed class RectangleRenderer : Renderer
    {
        public Texture2D Texture;
        public Vector2 Origin = Vector2.Zero;
        public Color TextureColor = Color.White;
        public int SortingLayer = 0;
        public bool Visible = true;
        public Transform transform;

        public override void Awake()
        {
            Texture = MonoEngine.WhitePixel;

            transform = GetOrAddComponent<Transform>();
        }

        public override bool IsVisible()
        {
            return Visible && spriteBatch != null && TextureColor.A != 0;
        }

        public override void Draw()
        {
            Vector2 pixelSize = new Vector2(transform.Scale.X / Texture.Width, transform.Scale.Y / Texture.Height);
            Vector2 pixelOrigin = new Vector2(Origin.X * Texture.Width, Origin.Y * Texture.Height);
            Vector2 flippedPosition = new Vector2(transform.Position.X, -transform.Position.Y);

            spriteBatch.Draw(
                Texture,
                flippedPosition,
                null,
                TextureColor,
                -transform.Rotation,
                pixelOrigin,
                pixelSize,
                SpriteEffects.None,
                SortingLayer
            );
        }
    }
}
