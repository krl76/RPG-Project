using Data.Configs;
using UnityEngine;

namespace Features.Enemy
{
    public partial class EnemyAI
    {
        private int _lastBossMeleeAudioClipIndex = -1;
        private int _lastBossMagicAudioClipIndex = -1;

        private AudioSource _stateAudioSource;
        private AudioSource _loopingStateAudioSource;

        private void PlayConfiguredMagicAttackSound()
        {
            bool shouldInterruptWithState = Config.BehaviourType == EnemyBehaviourType.Boss
                && (_currentAction == EnemyActionType.StrongAttack
                    || _currentAction == EnemyActionType.AirAttack);

            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                PlayBossAttackAudioCue(Config.MagicAttackSound, ref _lastBossMagicAudioClipIndex, shouldInterruptWithState);
                return;
            }

            PlayAudioCue(Config.MagicAttackSound, GetMagicAttackAudioVariationIndex(), shouldInterruptWithState);
        }

        private int GetMagicAttackAudioVariationIndex()
        {
            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                return Mathf.Max(0, _selectedBossElementIndex);
            }

            if (IsUsingRangedAttack())
            {
                return Mathf.Max(0, _selectedRegularVariationIndex);
            }

            return 0;
        }

        private bool PlayAudioCue(EnemyAudioCue cue, int variationIndex, bool interruptible = false)
        {
            if (_effectsAudioService == null || cue.TryGetClip(variationIndex, out AudioClip clip) == false)
            {
                return false;
            }

            return PlayResolvedAudioCue(clip, cue.Volume, interruptible);
        }

        private bool PlayBossAttackAudioCue(EnemyAudioCue cue, ref int lastClipIndex, bool interruptible)
        {
            if (_effectsAudioService == null || TryGetNextNonRepeatingBossAttackClip(cue, ref lastClipIndex, out AudioClip clip) == false)
            {
                return false;
            }

            return PlayResolvedAudioCue(clip, cue.Volume, interruptible);
        }

        private bool TryGetNextNonRepeatingBossAttackClip(EnemyAudioCue cue, ref int lastClipIndex, out AudioClip clip)
        {
            clip = null;

            if (cue.Clips == null || cue.Clips.Length == 0)
            {
                return false;
            }

            int selectedIndex = ChooseNextNonRepeatingValidClipIndex(cue.Clips, lastClipIndex);
            if (selectedIndex < 0)
            {
                return false;
            }

            lastClipIndex = selectedIndex;
            clip = cue.Clips[selectedIndex];
            return clip != null;
        }

        private bool PlayResolvedAudioCue(AudioClip clip, float volume, bool interruptible)
        {
            if (_effectsAudioService == null || clip == null)
            {
                return false;
            }

            float clampedVolume = Mathf.Clamp01(volume);

            if (interruptible && _stateAudioSource != null)
            {
                _stateAudioSource.Stop();
                _stateAudioSource.clip = clip;
                _stateAudioSource.volume = clampedVolume;
                _stateAudioSource.Play();
                return true;
            }

            _effectsAudioService.PlayOneShot(clip, clampedVolume);
            return true;
        }

        private void StopStateAudio()
        {
            if (_stateAudioSource == null)
            {
                return;
            }

            _stateAudioSource.Stop();
            _stateAudioSource.clip = null;
        }

        private bool PlayLoopingAudioCue(EnemyAudioCue cue, int variationIndex)
        {
            if (_loopingStateAudioSource == null || cue.TryGetClip(variationIndex, out AudioClip clip) == false)
            {
                return false;
            }

            _loopingStateAudioSource.Stop();
            _loopingStateAudioSource.clip = clip;
            _loopingStateAudioSource.volume = Mathf.Clamp01(cue.Volume);
            _loopingStateAudioSource.loop = true;
            _loopingStateAudioSource.Play();
            return true;
        }

        private static int ChooseNextNonRepeatingValidClipIndex(AudioClip[] clips, int previousIndex)
        {
            if (clips == null || clips.Length == 0)
            {
                return -1;
            }

            int validCount = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return -1;
            }

            if (validCount == 1)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null)
                    {
                        return i;
                    }
                }

                return -1;
            }

            int normalizedPreviousIndex = previousIndex >= 0
                && previousIndex < clips.Length
                && clips[previousIndex] != null
                ? previousIndex
                : -1;

            int candidateOrdinal = Random.Range(0, validCount - (normalizedPreviousIndex >= 0 ? 1 : 0));
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null || i == normalizedPreviousIndex)
                {
                    continue;
                }

                if (candidateOrdinal == 0)
                {
                    return i;
                }

                candidateOrdinal--;
            }

            return -1;
        }

        private void StopLoopingStateAudio()
        {
            if (_loopingStateAudioSource == null)
            {
                return;
            }

            _loopingStateAudioSource.Stop();
            _loopingStateAudioSource.loop = false;
            _loopingStateAudioSource.clip = null;
        }
    }
}
