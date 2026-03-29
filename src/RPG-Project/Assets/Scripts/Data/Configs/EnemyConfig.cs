using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Data.Configs
{
    public enum EnemyType
    {
        Melee,
        Ranged
    }

    public enum EnemyBehaviourType
    {
        Regular,
        Boss
    }

    public enum EnemyAttackDeliveryType
    {
        Auto,
        Melee,
        Ranged,
        SustainedEffect
    }

    [System.Serializable]
    public sealed class BossElementVariation
    {
        public string Id;
        public GameObject MeleeWeaponEffectPrefab;
        public GameObject SustainedAttackEffectPrefab;
        public GameObject SustainedAttackProjectilePrefab;
        public Vector3 SustainedAttackProjectileRotationOffset;
    }

    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("General")]
        public int Id;
        public EnemyType Type;
        public EnemyBehaviourType BehaviourType = EnemyBehaviourType.Regular;

        [Header("Stats")]
        [Min(1)] public float MaxHealth = 100f;
        [Min(0)] public float Damage = 15f;
        [Range(0f, 1f)] public float FleeHealthThreshold = 0.2f;
        [Range(0f, 1f)] public float EnrageHealthThreshold = 0.35f;

        [Header("Movement")]
        [Min(0f)] public float ChaseRange = 10f;
        [Min(0f)] public float DisengageRange = 14f;
        [Min(0f)] public float FleeDistance = 8f;
        [Min(0f)] public float FleeRepathInterval = 0.5f;
        [Min(0f)] public float RotationSpeed = 8f;
        [Min(0f)] public float ChaseSpeedMultiplier = 1f;
        [Min(0f)] public float FleeSpeedMultiplier = 1.2f;
        [Min(0f)] public float EnragedSpeedMultiplier = 1.15f;

        [Header("Combat")]
        public float AttackRange = 2f;
        [Min(0f)] public float StrongAttackRange = 6f;
        public float AttackCooldown = 2f;
        [Min(0f)] public float StrongAttackCooldown = 5f;
        public float HitRadius = 0.5f;
        public float StrongAttackDamageMultiplier = 1.8f;
        [Min(1f)] public float StrongAttackHitRadiusMultiplier = 1.5f;
        [Min(1f)] public float EnragedDamageMultiplier = 1.25f;
        [Min(0f)] public float RecoverDuration = 1f;
        [Range(0f, 1f)] public float DefendDamageTakenMultiplier = 0.5f;

        [Header("Ranged specific")]
        public GameObject ProjectilePrefab;
        public float ProjectileSpeed = 20f;
        public GameObject SustainedAttackEffectPrefab;
        public GameObject SustainedAttackProjectilePrefab;
        public Vector3 SustainedAttackProjectileRotationOffset;
        [Min(0.05f)] public float SustainedAttackProjectileLifetime = 0.75f;
        public EnemyAttackDeliveryType StrongAttackDeliveryType = EnemyAttackDeliveryType.Auto;
        [Min(1)] public int StrongAttackProjectileCount = 3;
        [Min(0f)] public float StrongAttackProjectileSpreadAngle = 12f;
        [Min(1f)] public float StrongAttackProjectileSpeedMultiplier = 1.1f;
        [Min(0f)] public float StrongAttackEffectRadius = 2.5f;
        [Min(0.05f)] public float StrongAttackDamageTickInterval = 0.2f;

        [Header("Boss Air Attack")]
        [Min(0f)] public float AirAttackRange = 14f;
        [Min(0f)] public float AirAttackCooldown = 8f;
        [Min(1f)] public float AirAttackDamageMultiplier = 1.5f;
        [Min(0f)] public float AirAttackEffectRadius = 3f;
        [Min(0.05f)] public float AirAttackDamageTickInterval = 0.2f;

        [Header("Visual Variations - Regular")]
        public List<GameObject> MeleeWeaponEffectPrefabs = new();
        public List<GameObject> RangedProjectilePrefabs = new();

        [Header("Visual Variations - Boss")]
        public List<BossElementVariation> BossElementVariations = new();

        [Header("Animation Fallback Durations")]
        [Min(0.1f)] public float AggressionAnimationDuration = 1.2f;
        [Min(0f)] public float AggressionPostActionDelay = 0.35f;
        [Min(0.1f)] public float AttackAnimationDuration = 1f;
        [Min(0.1f)] public float StrongAttackAnimationDuration = 1.4f;
        [Min(0.1f)] public float EnrageAnimationDuration = 1.4f;
        [Min(0f)] public float EnragePostActionDelay = 0.25f;
        [Min(0.1f)] public float AirAttackAnimationDuration = 4f;

#if UNITY_EDITOR
        private void Reset()
        {
            AssignUniqueId();
        }

        [ContextMenu("Recalculate ID")]
        private void AssignUniqueId()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyConfig");
            HashSet<int> usedIds = new HashSet<int>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyConfig data = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
                if (data != null && data != this) usedIds.Add(data.Id);
            }

            int nextId = 1;
            while (usedIds.Contains(nextId)) nextId++;

            if (Id != nextId)
            {
                Id = nextId;
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
