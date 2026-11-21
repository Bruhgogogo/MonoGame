using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Class
{
    public abstract class Renderer : Component
    {
        protected SpriteBatch spriteBatch;

        public override void Awake()
        {
            spriteBatch = MonoEngine.SpriteBatch;
            MonoEngine.RendererList.Add(this);
        }

        public abstract void Draw();
        public abstract bool IsVisible();
    }

    public sealed class RendererSprite : Renderer
    {
        public Texture2D Texture;
        public Vector2 Origin = Vector2.Zero;
        public Color TextureColor = Color.White;
        public float Rotation = 0;
        public int SortingLayer = 0;
        public bool FlipX = false, FlipY = false, Visible = true;

        private Vector2 position = Vector2.Zero;
        private Vector2 size = Vector2.One;
        private Rectangle rectangle = Rectangle.Empty;

        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                rectangle.X = (int)MathF.Round(value.X);
                rectangle.Y = (int)MathF.Round(value.Y);
            }
        }

        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                rectangle.Width = (int)MathF.Round(value.X);
                rectangle.Height = (int)MathF.Round(value.Y);
            }
        }

        public override bool IsVisible()
        {
            return Visible && Texture != null && spriteBatch != null && TextureColor.A != 0;
        }

        public override void Draw()
        {
            if (Texture == null || spriteBatch == null)
                return;

            spriteBatch.Draw(Texture, rectangle, null, TextureColor, Rotation, Origin, GetSpriteEffect(), SortingLayer);
        }

        private SpriteEffects GetSpriteEffect()
        {
            SpriteEffects flag = SpriteEffects.None;
            if (FlipX) flag |= SpriteEffects.FlipHorizontally;
            if (FlipY) flag |= SpriteEffects.FlipVertically;
            return flag;
        }
    }

    public sealed class RendererText : Renderer
    {
        public SpriteFont Font;
        public Vector2 Origin = Vector2.Zero, Position = Vector2.Zero;
        public Color TextColor = Color.White;
        public string Text = "";
        public float Rotation = 0, Scale = 1.0f;
        public int SortingLayer = 0;
        public bool FlipX = false, FlipY = false, Visible = true;

        public override bool IsVisible()
        {
            return Visible && Font != null && spriteBatch != null && !string.IsNullOrEmpty(Text) && TextColor.A != 0;
        }

        public override void Draw()
        {
            if (Font == null || spriteBatch == null || string.IsNullOrEmpty(Text))
                return;

            spriteBatch.DrawString(Font, Text, Position, TextColor, Rotation, Origin, Scale, GetSpriteEffect(), SortingLayer);
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