using Microsoft.Xna.Framework;

using MonoAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

namespace MonoEngine.Class
{
    public class AudioListener : Behaviour
    {
        public enum AudioVelocityUpdateMode
        {
            FixedUpdate,
            Update,
            Manuel
        }

        public static AudioListener Main;

        public AudioVelocityUpdateMode Mode = AudioVelocityUpdateMode.Update;

        private MonoAudioListener monoListener = new MonoAudioListener();
        public Vector2 Velocity = Vector2.Zero;

        public Transform transform;
        private Vector2 lastPosition = Vector2.Zero;

        private void UpdateVelocity(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (deltaTime > 0f)
            {
                Velocity = (transform.Position - lastPosition) / deltaTime;
            }

            lastPosition = transform.Position;
        }

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

        public override void Update(GameTime gameTime)
        {
            if (Mode == AudioVelocityUpdateMode.Update)
                UpdateVelocity(gameTime);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            if (Mode == AudioVelocityUpdateMode.FixedUpdate)
                UpdateVelocity(gameTime);
        }

        public Vector3 GetPosition()
        {
            return new Vector3(transform.Position.X, transform.Position.Y, 0f);
        }

        public Vector3 GetForward()
        {
            Vector2 forward = transform.GetForward();
            return new Vector3(forward.X, forward.Y, 0f);
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

        public MonoAudioListener GetMonoListener()
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