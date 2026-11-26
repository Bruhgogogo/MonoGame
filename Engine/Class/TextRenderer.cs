using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoEngine.Class
{
    public sealed class TextRenderer : Renderer
    {
        public SpriteFont Font;
        public Vector2 Origin = Vector2.Zero;
        public Color TextColor = Color.White;
        public string Text = "";
        public int SortingLayer = 0;
        public bool FlipX = false, FlipY = false, Visible = true;
        public Transform transform;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
        }

        public override bool IsVisible()
        {
            return Visible && Font != null && spriteBatch != null && !string.IsNullOrEmpty(Text) && TextColor.A != 0;
        }

        public override void Draw()
        {
            float biggestScale = MathF.Max(transform.Scale.X, transform.Scale.Y);
            Vector2 pixelOrigin = new Vector2(Origin.X * biggestScale, Origin.Y * biggestScale);
            Vector2 flippedPosition = new Vector2(transform.Position.X, -transform.Position.Y);

            spriteBatch.DrawString(
                Font,
                Text,
                flippedPosition,
                TextColor,
                -transform.Rotation,
                pixelOrigin,
                biggestScale,
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