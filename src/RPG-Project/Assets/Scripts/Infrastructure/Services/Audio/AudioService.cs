using UnityEngine;
using UnityEngine.Audio;

namespace Infrastructure.Services.Audio
{
    /// <summary>
    /// Управляет пользовательскими уровнями громкости через AudioMixer.
    /// </summary>
    public sealed class AudioService : IAudioService
    {
        private const string MasterVolumeKey = "audio.master";
        private const string MusicVolumeKey = "audio.music";
        private const string EffectsVolumeKey = "audio.effects";
        private const string MasterVolumeParameter = "MasterVolume";
        private const string MusicVolumeParameter = "MusicVolume";
        private const string EffectsVolumeParameter = "EffectsVolume";
        private const float MinDecibels = -80f;

        public event System.Action VolumesChanged;

        private readonly AudioMixer _audioMixer;

        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float EffectsVolume { get; private set; }

        public AudioService(AudioMixer audioMixer)
        {
            _audioMixer = audioMixer;

            MasterVolume = LoadVolume(MasterVolumeKey);
            MusicVolume = LoadVolume(MusicVolumeKey);
            EffectsVolume = LoadVolume(EffectsVolumeKey);

            ApplyVolumes();
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
            ApplyVolumes();
            VolumesChanged?.Invoke();
        }

        private void ApplyVolumes()
        {
            if (_audioMixer == null)
            {
                Debug.LogWarning("[AudioService] AudioMixer is not assigned.");
                return;
            }

            TrySetMixerVolume(MasterVolumeParameter, MasterVolume);
            TrySetMixerVolume(MusicVolumeParameter, MusicVolume);
            TrySetMixerVolume(EffectsVolumeParameter, EffectsVolume);
        }

        private void TrySetMixerVolume(string parameterName, float value)
        {
            if (_audioMixer.SetFloat(parameterName, LinearToDecibels(value)) == false)
            {
                Debug.LogWarning($"[AudioService] Exposed parameter '{parameterName}' was not found in AudioMixer.");
            }
        }

        private static float LinearToDecibels(float value)
        {
            if (value <= 0.0001f)
            {
                return MinDecibels;
            }

            return Mathf.Log10(value) * 20f;
        }

        private static float LoadVolume(string key) =>
            Mathf.Clamp01(PlayerPrefs.GetFloat(key, 1f));
    }
}
