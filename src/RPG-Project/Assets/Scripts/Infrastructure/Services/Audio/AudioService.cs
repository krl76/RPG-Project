using UnityEngine;

namespace Infrastructure.Services.Audio
{
    public sealed class AudioService : IAudioService
    {
        private const string MasterVolumeKey = "audio.master";
        private const string MusicVolumeKey = "audio.music";
        private const string EffectsVolumeKey = "audio.effects";

        public event System.Action VolumesChanged;

        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float EffectsVolume { get; private set; }

        public AudioService()
        {
            MasterVolume = LoadVolume(MasterVolumeKey);
            MusicVolume = LoadVolume(MusicVolumeKey);
            EffectsVolume = LoadVolume(EffectsVolumeKey);
        }

        public float GetEffectiveVolume(AudioChannel channel)
        {
            return channel switch
            {
                AudioChannel.Master => MasterVolume,
                AudioChannel.Music => MasterVolume * MusicVolume,
                AudioChannel.Effects => MasterVolume * EffectsVolume,
                _ => 1f
            };
        }

        public void SetMasterVolume(float value) =>
            SetVolume(MasterVolumeKey, Mathf.Clamp01(value), current => MasterVolume = current, MasterVolume);

        public void SetMusicVolume(float value) =>
            SetVolume(MusicVolumeKey, Mathf.Clamp01(value), current => MusicVolume = current, MusicVolume);

        public void SetEffectsVolume(float value) =>
            SetVolume(EffectsVolumeKey, Mathf.Clamp01(value), current => EffectsVolume = current, EffectsVolume);

        private void SetVolume(string key, float value, System.Action<float> assign, float currentValue)
        {
            if (Mathf.Approximately(currentValue, value))
            {
                return;
            }

            assign(value);
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            VolumesChanged?.Invoke();
        }

        private static float LoadVolume(string key) =>
            Mathf.Clamp01(PlayerPrefs.GetFloat(key, 1f));
    }
}
