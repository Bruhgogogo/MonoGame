using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public class AudioListener : Component
    {
        public static AudioListener Main;

        private Microsoft.Xna.Framework.Audio.AudioListener monoListener = new Microsoft.Xna.Framework.Audio.AudioListener();
        public Vector2 Velocity = Vector2.Zero;

        private Transform transform;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();

            if (!Main)
                Main = this;
        }

        public override void OnDestroy()
        {
            if (Main == this)
                Main = null;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(transform.Position.X, transform.Position.Y, 0f);
        }

        public Vector3 GetForward()
        {
            Vector2 foward = transform.GetForward();
            return new Vector3(foward.X, foward.Y, 0f);
        }

        public Vector3 GetUp()
        {
            Vector2 up = transform.GetUp();
            return new Vector3(up.X, up.Y, 0f);
        }

        public Vector3 GetVelocity()
        {
            return new Vector3(Velocity.X, Velocity.Y, 0f);
        }

        public Microsoft.Xna.Framework.Audio.AudioListener GetMonoListener()
        {
            Vector2 fwd = transform.GetForward();
            Vector2 up = transform.GetUp();

            monoListener.Position = new Vector3(transform.Position.X, transform.Position.Y, 0f);
            monoListener.Forward = new Vector3(fwd.X, fwd.Y, 0f);
            monoListener.Up = new Vector3(up.X, up.Y, 0f);
            monoListener.Velocity = new Vector3(Velocity.X, Velocity.Y, 0f);
            return monoListener;
        }
    }
}