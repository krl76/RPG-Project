using Data.Configs;
using Infrastructure.Providers.Configs;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Audio
{
    public sealed class GameplayAudioService : IGameplayAudioService, System.IDisposable
    {
        private readonly IConfigDataProvider _configDataProvider;
        private readonly IEffectsAudioService _effectsAudioService;
        private readonly AudioSource _musicAudioSource;

        public GameplayAudioService(
            IConfigDataProvider configDataProvider,
            IEffectsAudioService effectsAudioService,
            AudioMixer audioMixer)
        {
            _configDataProvider = configDataProvider;
            _effectsAudioService = effectsAudioService;

            var audioRoot = new GameObject("[GameplayAudioService]");
            Object.DontDestroyOnLoad(audioRoot);

            _musicAudioSource = audioRoot.AddComponent<AudioSource>();
            ConfigureAudioSource(_musicAudioSource);

            if (audioMixer == null)
            {
                Debug.LogWarning("[GameplayAudioService] AudioMixer is not assigned.");
                return;
            }

            var groups = audioMixer.FindMatchingGroups("Music");
            if (groups.Length == 0)
            {
                Debug.LogWarning("[GameplayAudioService] AudioMixerGroup 'Music' was not found.");
                return;
            }

            _musicAudioSource.outputAudioMixerGroup = groups[0];
        }

        public void PlayScoreGained() =>
            PlayEffectCue(_configDataProvider.GetCombatAudioConfig()?.ScoreGained);

        public void PlayVictoryMusic()
        {
            if (TryGetClip(_configDataProvider.GetCombatAudioConfig()?.VictoryMusic, out AudioClip clip, out float volume) == false)
            {
                return;
            }

            _musicAudioSource.Stop();
            _musicAudioSource.clip = clip;
            _musicAudioSource.volume = volume;
            _musicAudioSource.Play();
        }

        public void StopVictoryMusic()
        {
            if (_musicAudioSource == null)
            {
                return;
            }

            _musicAudioSource.Stop();
            _musicAudioSource.clip = null;
        }

        public void Dispose()
        {
            if (_musicAudioSource != null)
            {
                Object.Destroy(_musicAudioSource.gameObject);
            }
        }

        private void PlayEffectCue(CombatAudioConfig.AudioCue? cue)
        {
            if (TryGetClip(cue, out AudioClip clip, out float volume) == false)
            {
                return;
            }

            _effectsAudioService.PlayOneShot(clip, volume);
        }

        private static bool TryGetClip(CombatAudioConfig.AudioCue? cue, out AudioClip clip, out float volume)
        {
            clip = null;
            volume = 1f;

            if (cue == null || cue.Value.Clips == null || cue.Value.Clips.Length == 0)
            {
                return false;
            }

            volume = Mathf.Clamp01(cue.Value.Volume);
            int validClipCount = 0;
            for (int i = 0; i < cue.Value.Clips.Length; i++)
            {
                if (cue.Value.Clips[i] != null)
                {
                    validClipCount++;
                }
            }

            if (validClipCount == 0)
            {
                return false;
            }

            int targetIndex = Random.Range(0, validClipCount);
            for (int i = 0; i < cue.Value.Clips.Length; i++)
            {
                AudioClip candidate = cue.Value.Clips[i];
                if (candidate == null)
                {
                    continue;
                }

                if (targetIndex == 0)
                {
                    clip = candidate;
                    return true;
                }

                targetIndex--;
            }

            return false;
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
