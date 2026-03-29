using System;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Audio
{
    public sealed class EffectsAudioService : IEffectsAudioService, IDisposable
    {
        private readonly AudioSource _audioSource;
        private readonly AudioMixerGroup _sfxMixerGroup;

        public EffectsAudioService(AudioMixer audioMixer)
        {
            var audioRoot = new GameObject("[EffectsAudioService]");
            Object.DontDestroyOnLoad(audioRoot);

            _audioSource = audioRoot.AddComponent<AudioSource>();
            ConfigureAudioSource(_audioSource);

            if (audioMixer == null)
            {
                Debug.LogWarning("[EffectsAudioService] AudioMixer is not assigned.");
                return;
            }

            var groups = audioMixer.FindMatchingGroups("SFX");
            if (groups.Length == 0)
            {
                Debug.LogWarning("[EffectsAudioService] AudioMixerGroup 'SFX' was not found.");
                return;
            }

            _sfxMixerGroup = groups[0];
            _audioSource.outputAudioMixerGroup = _sfxMixerGroup;
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public AudioSource CreateConfiguredSource(Transform parent = null, string sourceName = null)
        {
            var sourceObject = new GameObject(string.IsNullOrWhiteSpace(sourceName) ? "[EffectsAudioSource]" : sourceName);
            if (parent != null)
            {
                sourceObject.transform.SetParent(parent, false);
            }

            var source = sourceObject.AddComponent<AudioSource>();
            ConfigureAudioSource(source);
            source.outputAudioMixerGroup = _sfxMixerGroup;
            return source;
        }

        public void Dispose()
        {
            if (_audioSource != null)
            {
                Object.Destroy(_audioSource.gameObject);
            }
        }

        private static void ConfigureAudioSource(AudioSource audioSource)
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }
}
