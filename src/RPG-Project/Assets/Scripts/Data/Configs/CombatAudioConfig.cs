using System;
using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(menuName = "Configs/Combat Audio Config", fileName = "CombatAudioConfig")]
    public sealed class CombatAudioConfig : ScriptableObject
    {
        [Serializable]
        public struct AudioCue
        {
            [Range(0f, 1f)] public float Volume;
            public AudioClip[] Clips;
        }

        [Header("Player")]
        public AudioCue PlayerMeleeAttack;
        public AudioCue PlayerShot;
        public AudioCue PlayerHit;

        [Header("Enemy")]
        public AudioCue EnemyMeleeAttack;
        public AudioCue EnemyMagicAttack;
        public AudioCue EnemyHit;
    }
}
