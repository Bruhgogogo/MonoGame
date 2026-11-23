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

        public Vector2 Velocity = Vector2.Zero;

        public Transform transform;
        private Vector2 lastPosition = Vector2.Zero;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
            lastPosition = transform.Position;

            if (!Main)
                Main = this;
        }

        protected override void OnDestroy()
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

        private void UpdateVelocity(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (deltaTime > 0f)
            {
                Velocity = (transform.Position - lastPosition) / deltaTime;
            }

            lastPosition = transform.Position;
        }
    }
}