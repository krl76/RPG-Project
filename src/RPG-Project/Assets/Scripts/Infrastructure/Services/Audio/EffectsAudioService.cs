using System;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Audio
{
    public sealed class EffectsAudioService : IEffectsAudioService, IDisposable
    {
        private readonly AudioSource _audioSource;

        public EffectsAudioService(AudioMixer audioMixer)
        {
            var audioRoot = new GameObject("[EffectsAudioService]");
            Object.DontDestroyOnLoad(audioRoot);

            _audioSource = audioRoot.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;

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

            _audioSource.outputAudioMixerGroup = groups[0];
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void Dispose()
        {
            if (_audioSource != null)
            {
                Object.Destroy(_audioSource.gameObject);
            }
        }
    }
}
