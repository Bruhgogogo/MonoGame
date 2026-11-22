using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Class
{
    public sealed class Camera : Component
    {
        public static Camera Main;

        public Transform Transform;

        public override void Awake()
        {
            Transform = GetOrAddComponent<Transform>();

            if (!Main)
                Main = this;
        }

        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(-Transform.Position.X, -Transform.Position.Y, 0.0f)
                * Matrix.CreateRotationZ(Transform.Rotation)
                * Matrix.CreateScale(Transform.Scale.X, Transform.Scale.Y, 1.0f)
                * Matrix.CreateTranslation(new Vector3(MonoEngine.ViewportSize * 0.5f, 0.0f));
        }
    }
}
