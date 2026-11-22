using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Class
{
    public sealed class Transform : Component
    {
        public Vector2 Position = Vector2.Zero;
        public Vector2 Scale = Vector2.One;
        public float Rotation = 0.0f;

        public Vector2 GetForward()
        {
            return new Vector2(
                MathF.Cos(Rotation),
                -MathF.Sin(Rotation)
            );
        }

        public Vector2 GetUp()
        {
            Vector2 forward = GetForward();
            return new Vector2(-forward.Y, forward.X);
        }
    }
}