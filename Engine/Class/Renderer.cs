using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

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
            spriteBatch.Draw(
                Texture,
                transform.Position,
                null,
                TextureColor,
                transform.Rotation,
                Origin,
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

    public sealed class RendererText : Renderer
    {
        public SpriteFont Font;
        public Vector2 Origin = Vector2.Zero;
        public Color TextColor = Color.White;
        public string Text = "";
        public int SortingLayer = 0;
        public bool FlipX = false, FlipY = false, Visible = true;

        private Transform transform;

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
            spriteBatch.DrawString(
                Font,
                Text,
                transform.Position,
                TextColor,
                transform.Rotation,
                Origin,
                MathF.Max(transform.Scale.X, transform.Scale.Y),
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