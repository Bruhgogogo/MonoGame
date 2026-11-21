using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public sealed class Transform : Component
    {
        public Vector2 Position = Vector2.Zero;
        public Vector2 Scale = Vector2.One;
        public float Rotation = 0.0f;
    }
}