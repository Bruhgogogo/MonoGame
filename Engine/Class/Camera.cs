using Microsoft.Xna.Framework;

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

        public override void OnDestroy()
        {
            if (Main == this)
                Main = null;
        }

        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(-Transform.Position.X, -Transform.Position.Y, 0.0f)
                * Matrix.CreateRotationZ(Transform.Rotation)
                * Matrix.CreateScale(Transform.Scale.X, Transform.Scale.Y, 1.0f)
                * Matrix.CreateTranslation(new Vector3(MonoEngine.ViewportSize * 0.5f, 0.0f));
        }

        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            Matrix viewMatrix = GetViewMatrix();
            Matrix inverseView = Matrix.Invert(viewMatrix);

            Vector3 screenPos3 = new Vector3(screenPos, 0f);
            Vector3 worldPos = Vector3.Transform(screenPos3, inverseView);

            return new Vector2(worldPos.X, worldPos.Y);
        }
    }
}
