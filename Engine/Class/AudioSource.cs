using Microsoft.Xna.Framework;
using MonoAudio = Microsoft.Xna.Framework.Audio;

namespace MonoEngine.Class
{
    public class AudioSource : Behaviour
    {
        public enum AudioVelocityUpdateMode
        {
            FixedUpdate,
            Update,
            Manuel
        }

        public MonoAudio.SoundEffect Clip;
        public Transform transform;

        public Vector2 Velocity = Vector2.Zero;

        public float Volume = 1.0f;
        public float Pitch = 0.0f;
        public bool Loop = false;
        public bool Is3D = false;
        public float MinDistance = 10f;
        public float MaxDistance = 500f;
        public float DopplerScale = 1.0f;

        public AudioVelocityUpdateMode Mode = AudioVelocityUpdateMode.Update;

        private MonoAudio.SoundEffectInstance instance;
        private MonoAudio.AudioEmitter emitter;

        private Vector2 lastPosition = Vector2.Zero;

        private bool isPlaying = false;
        public bool IsPlaying => isPlaying;

        public override void Start()
        {
            if (Is3D)
            {
                emitter = new MonoAudio.AudioEmitter();
                transform = GetOrAddComponent<Transform>();
            }
        }

        public void Play()
        {
            if (Clip == null) return;

            Stop();

            instance = Clip.CreateInstance();
            instance.Volume = Volume;
            instance.Pitch = Pitch;
            instance.IsLooped = Loop;

            if (Is3D && emitter != null)
            {
                UpdateSpatialData();
            }

            instance.Play();
            isPlaying = true;
        }

        public void Stop()
        {
            if (instance != null && !instance.IsDisposed)
            {
                instance.Stop();
                instance.Dispose();
                instance = null;
            }
            isPlaying = false;
        }

        public void Pause()
        {
            if (instance != null && !instance.IsDisposed && isPlaying)
            {
                instance.Pause();
                isPlaying = false;
            }
        }

        public void Resume()
        {
            if (instance != null && !instance.IsDisposed && !isPlaying)
            {
                instance.Resume();
                isPlaying = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!Is3D || instance == null || instance.IsDisposed || !isPlaying)
                return;

            if (Mode == AudioVelocityUpdateMode.Update)
            {
                CalculateVelocity(gameTime);
            }

            UpdateSpatialData();

            if (!Loop && instance.State == MonoAudio.SoundState.Stopped)
            {
                isPlaying = false;
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            if (!Is3D || instance == null || instance.IsDisposed || !isPlaying)
                return;

            if (Mode == AudioVelocityUpdateMode.FixedUpdate)
            {
                CalculateVelocity(gameTime);
            }
        }

        private void CalculateVelocity(GameTime gameTime)
        {
            if (transform)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (deltaTime > 0f)
                {
                    Velocity = (transform.Position - lastPosition) / deltaTime;
                }

                lastPosition = transform.Position;
            }
        }

        private void UpdateSpatialData()
        {
            if (transform)
            {
                Vector2 forward = transform.GetForward();
                Vector2 up = transform.GetUp();

                emitter.Position = new Vector3(transform.Position.X, transform.Position.Y, 0f);
                emitter.Forward = new Vector3(forward.X, forward.Y, 0f);
                emitter.Up = new Vector3(up.X, up.Y, 0f);
                emitter.Velocity = new Vector3(Velocity.X, Velocity.Y, 0f);
                emitter.DopplerScale = DopplerScale;
            }

            AudioListener mainListener = AudioListener.Main;
            if (mainListener)
            {
                instance.Apply3D(mainListener.GetMonoListener(), emitter);
            }
        }

        public override void OnDestroy()
        {
            Stop();
        }
    }
}