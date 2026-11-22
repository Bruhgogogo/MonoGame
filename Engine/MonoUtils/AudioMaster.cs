using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MonoEngine.MonoUtils
{
    public static class AudioMaster
    {
        public static void PlayOnce(SoundEffect clip, float volume = 1.0f, float pitch = 0.0f)
        {
            if (clip == null) return;

            SoundEffectInstance instance = clip.CreateInstance();
            instance.Volume = MathHelper.Clamp(volume, 0f, 1f);
            instance.Pitch = MathHelper.Clamp(pitch, -1f, 1f);
            instance.Play();

            instance.Dispose();
        }

        public static void PlayOnceAt(SoundEffect clip, Vector2 worldPosition, float volume = 1.0f, float pitch = 0.0f)
        {
            if (clip == null) return;

            Class.AudioListener mainListener = Class.AudioListener.Main;
            if (!mainListener) return;

            SoundEffectInstance instance = clip.CreateInstance();
            instance.Volume = MathHelper.Clamp(volume, 0f, 1f);
            instance.Pitch = MathHelper.Clamp(pitch, -1f, 1f);

            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = new Vector3(worldPosition.X, worldPosition.Y, 0f);

            Microsoft.Xna.Framework.Audio.AudioListener listener = new Microsoft.Xna.Framework.Audio.AudioListener();

            listener = mainListener.GetMonoListener();
            instance.Apply3D(listener, emitter);
            instance.Play();
            instance.Dispose();
        }
    }
}