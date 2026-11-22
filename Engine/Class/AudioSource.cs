using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MonoEngine.Class
{
    public class AudioSource : Behaviour
    {
        public SoundEffect Clip;
        public float Volume = 1.0f;
        public float Pitch = 0.0f;
        public bool Loop = false;
        public bool Spatial = false;
        public float MinDistance = 10f;
        public float MaxDistance = 500f;

        private SoundEffectInstance instance;
        private AudioEmitter emitter;

        private bool isPlaying = false;
        public bool IsPlaying => isPlaying;

        public override void Awake()
        {
            if (Spatial)
            {
                emitter = new AudioEmitter();
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

            if (Spatial && emitter != null)
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
            if (!Spatial || instance == null || instance.IsDisposed || !isPlaying)
                return;

            UpdateSpatialData();

            if (!Loop && instance.State == SoundState.Stopped)
            {
                isPlaying = false;
            }
        }

        private void UpdateSpatialData()
        {
            Transform transform = GetComponent<Transform>();
            if (transform != null)
            {
                emitter.Position = new Vector3(transform.Position.X, transform.Position.Y, 0f);
                emitter.Forward = Vector3.Forward;
                emitter.Up = Vector3.Up;
                emitter.Velocity = Vector3.Zero;
            }

            Class.AudioListener mainListener = Class.AudioListener.Main;
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