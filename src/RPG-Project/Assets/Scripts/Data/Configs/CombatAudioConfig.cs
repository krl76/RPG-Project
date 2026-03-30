using System;
using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(menuName = "Configs/Combat Audio Config", fileName = "CombatAudioConfig")]
    /// <summary>
    /// Конфиг звуков боя и прогрессии.
    /// </summary>
    public sealed class CombatAudioConfig : ScriptableObject
    {
        [Serializable]
        /// <summary>
        /// Набор аудиоклипов с общей громкостью для конкретного действия.
        /// </summary>
        public struct AudioCue
        {
            [Range(0f, 1f)] public float Volume;
            public AudioClip[] Clips;
        }

        [Header("Player")]
        public AudioCue PlayerMeleeAttack;
        public AudioCue PlayerShot;
        public AudioCue PlayerHit;

        [Header("Progression")]
        public AudioCue ScoreGained;
        public AudioCue VictoryMusic;
    }
}
