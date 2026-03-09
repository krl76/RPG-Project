using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Data.Configs
{
    public enum EnemyType { Melee, Ranged }
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("General")]
        public int Id;
        public EnemyType Type;

        [Header("Stats")]
        [Min(1)] public float MaxHealth = 100f;
        [Min(0)] public float Damage = 15f;

        [Header("Combat Ranges")]
        public float ChaseRange = 10f;
        public float AttackRange = 2f;
        public float AttackCooldown = 2f;
        public float HitRadius = 0.5f;

        [Header("Ranged specific")]
        public GameObject ProjectilePrefab;

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