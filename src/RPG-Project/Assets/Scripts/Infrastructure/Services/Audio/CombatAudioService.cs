using Data.Configs;
using Infrastructure.Providers.Configs;
using UnityEngine;

namespace Infrastructure.Services.Audio
{
    public sealed class CombatAudioService : ICombatAudioService
    {
        private readonly IConfigDataProvider _configDataProvider;
        private readonly IEffectsAudioService _effectsAudioService;

        public CombatAudioService(
            IConfigDataProvider configDataProvider,
            IEffectsAudioService effectsAudioService)
        {
            _configDataProvider = configDataProvider;
            _effectsAudioService = effectsAudioService;
        }

        public void PlayPlayerMeleeAttack() => PlayCue(_configDataProvider.GetCombatAudioConfig()?.PlayerMeleeAttack);
        public void PlayPlayerShot() => PlayCue(_configDataProvider.GetCombatAudioConfig()?.PlayerShot);
        public void PlayPlayerHit() => PlayCue(_configDataProvider.GetCombatAudioConfig()?.PlayerHit);
        public void PlayEnemyMeleeAttack() => PlayCue(_configDataProvider.GetCombatAudioConfig()?.EnemyMeleeAttack);
        public void PlayEnemyMagicAttack() => PlayCue(_configDataProvider.GetCombatAudioConfig()?.EnemyMagicAttack);
        public void PlayEnemyHit() => PlayCue(_configDataProvider.GetCombatAudioConfig()?.EnemyHit);

        private void PlayCue(CombatAudioConfig.AudioCue? cue)
        {
            if (cue == null || cue.Value.Clips == null || cue.Value.Clips.Length == 0)
            {
                return;
            }

            var clips = cue.Value.Clips;
            var clip = clips[Random.Range(0, clips.Length)];
            var volume = Mathf.Clamp01(cue.Value.Volume);

            _effectsAudioService.PlayOneShot(clip, volume);
        }
    }
}
