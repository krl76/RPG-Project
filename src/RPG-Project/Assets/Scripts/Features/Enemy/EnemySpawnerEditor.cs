#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Features.Enemy
{
    [CustomEditor(typeof(EnemySpawner))]
    public sealed class EnemySpawnerEditor : Editor
    {
        private void OnSceneGUI()
        {
            EnemySpawner spawner = (EnemySpawner)target;
            IReadOnlyList<EnemySpawnPointData> spawnPoints = spawner.SpawnPoints;
            if (spawnPoints == null)
            {
                return;
            }

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                EnemySpawnPointData spawnPoint = spawnPoints[i];
                if (spawnPoint == null)
                {
                    continue;
                }

                Vector3 worldCenter = spawnPoint.Center != null
                    ? spawnPoint.Center.position
                    : spawner.transform.position;

                EditorGUI.BeginChangeCheck();
                float nextRadius = Handles.RadiusHandle(Quaternion.identity, worldCenter, spawnPoint.Radius);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spawner, "Resize Enemy Spawn Radius");
                    spawnPoint.Radius = Mathf.Max(0.5f, nextRadius);
                    EditorUtility.SetDirty(spawner);
                }

                Handles.color = new Color(0.2f, 0.85f, 0.35f, 1f);
                Handles.Label(worldCenter + Vector3.up * 0.3f, $"Spawn Point {i}");
            }
        }
    }
}
#endif
