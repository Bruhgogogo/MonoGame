using Microsoft.Xna.Framework;
using System;
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
        public float MinDistance = 250.0f;
        public float MaxDistance = 5000.0f;
        public float DopplerScale = 1.0f;

        public AudioVelocityUpdateMode Mode = AudioVelocityUpdateMode.Update;

        private MonoAudio.SoundEffectInstance instance;
        private Vector2 lastPosition = Vector2.Zero;

        private const float SPEED_OF_SOUND = 343.0f;

        private bool isPlaying = false;
        public bool IsPlaying => isPlaying;

        private bool is3D = false;
        public bool Is3D
        {
            get
            {
                return is3D;
            }
            set
            {
                if (Started)
                {
                    return;
                }
                is3D = value;
            }
        }

        public override void Start()
        {
            if (is3D)
            {
                transform = GetOrAddComponent<Transform>();
                lastPosition = transform.Position;
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
            
            instance.Play();
            isPlaying = true;
        }

        public void Stop()
        {
            if (AudioInstanceExist())
            {
                instance.Stop();
                instance.Dispose();
                instance = null;
            }
            isPlaying = false;
        }

        public void Pause()
        {
            if (AudioInstanceExist() && isPlaying)
            {
                instance.Pause();
                isPlaying = false;
            }
        }

        public void Resume()
        {
            if (AudioInstanceExist() && !isPlaying)
            {
                instance.Resume();
                isPlaying = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!Is3DValid() || !AudioInstanceExist() || !isPlaying)
                return;

            if (Mode == AudioVelocityUpdateMode.Update)
            {
                CalculateVelocity(gameTime);
            }

            UpdateDopplerAndAttenuation();

            if (!Loop && instance.State == MonoAudio.SoundState.Stopped)
            {
                isPlaying = false;
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            if (!Is3DValid() || !AudioInstanceExist() || !isPlaying)
                return;

            if (Mode == AudioVelocityUpdateMode.FixedUpdate)
            {
                CalculateVelocity(gameTime);
            }
        }

        private bool AudioInstanceExist()
        {
            return instance != null && !instance.IsDisposed;
        }

        private bool Is3DValid()
        {
            return is3D && transform;
        }

        private void CalculateVelocity(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (deltaTime > 0f)
            {
                Velocity = (transform.Position - lastPosition) / deltaTime;
            }

            lastPosition = transform.Position;
        }

        private void UpdateDopplerAndAttenuation()
        {
            AudioListener mainListener = AudioListener.Main;
            if (!mainListener || !mainListener.transform)
                return;

            Vector2 relativePos = transform.Position - mainListener.transform.Position;
            float distance = relativePos.Length();

            float volumeFactor = 1.0f;
            if (distance > MinDistance)
            {
                float t = (distance - MinDistance) / (MaxDistance - MinDistance);
                volumeFactor = Math.Max(1.0f - t, 0f);
            }
            instance.Volume = Volume * volumeFactor;

            if (distance > 0.001f)
            {
                Vector2 radialDir = relativePos / distance;
                Vector2 normalizedAudioVelocity = Velocity * MonoEngine.PIXEL_TO_METER;
                Vector2 normalizedListenerVelocity = mainListener.Velocity * MonoEngine.PIXEL_TO_METER;

                float sourceRadialVel = Vector2.Dot(normalizedAudioVelocity, radialDir);
                float listenerRadialVel = Vector2.Dot(normalizedListenerVelocity, radialDir);

                float factor = (SPEED_OF_SOUND + listenerRadialVel) / (SPEED_OF_SOUND + sourceRadialVel);

                float scaledFactor = 1.0f + (factor - 1.0f) * DopplerScale;

                instance.Pitch = (1.0f + Pitch) * scaledFactor - 1.0f;
                instance.Pitch = MathHelper.Clamp(instance.Pitch, -1.0f, 1.0f);
            }
            else
            {
                instance.Pitch = Pitch;
            }
        }

        protected override void OnDestroy()
        {
            Stop();
        }
    }
}