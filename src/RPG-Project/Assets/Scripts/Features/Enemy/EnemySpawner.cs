using System;
using System.Collections.Generic;
using Data.Configs;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Enemy;
using Infrastructure.Services.Player;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Features.Enemy
{
    [Serializable]
    public sealed class EnemySpawnEntry
    {
        public EnemyConfig Config;
        [Min(1)] public int Count = 1;
    }

    [Serializable]
    public sealed class EnemySpawnPointData
    {
        public Transform Center;
        [Min(0.5f)] public float Radius = 4f;
        public List<EnemySpawnEntry> Enemies = new();
    }

    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const float MinSpawnRadius = 0.25f;
        private const float MinTriangleArea = 0.0001f;

        [Header("Spawn Setup")]
        [SerializeField] private Transform _spawnRoot;
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField] private List<EnemySpawnPointData> _spawnPoints = new();

        [Header("Random Spawn")]
        [SerializeField, Min(0)] private int _randomSpawnCount;
        [SerializeField] private List<EnemyConfig> _randomEnemyConfigs = new();

        [Header("Placement")]
        [SerializeField, Min(1)] private int _spawnAttemptsPerEnemy = 32;
        [SerializeField, Min(0f)] private float _spawnSpacingPadding = 0.15f;
        [SerializeField, Min(0.1f)] private float _navMeshSampleDistance = 2f;

        private readonly List<GameObject> _spawnedEnemies = new();
        private readonly List<SpawnReservation> _spawnReservations = new();

        private IEnemyService _enemyService;
        private IGameObjectFactory _gameObjectFactory;
        private IPlayerService _playerService;
        private bool _hasSpawned;

        [Inject]
        private void Construct(
            IEnemyService enemyService,
            IGameObjectFactory gameObjectFactory,
            IPlayerService playerService)
        {
            _enemyService = enemyService;
            _gameObjectFactory = gameObjectFactory;
            _playerService = playerService;
        }

        private void Start()
        {
            TryAutoSpawn();
        }

        [ContextMenu("Spawn")]
        private void SpawnFromContextMenu()
        {
            if (Application.isPlaying == false)
            {
                Debug.LogWarning("[EnemySpawner] Spawn via context menu is available only in Play Mode.", this);
                return;
            }

            Spawn();
        }

        [ContextMenu("Clear Spawned")]
        private void ClearSpawnedFromContextMenu()
        {
            if (Application.isPlaying == false)
            {
                Debug.LogWarning("[EnemySpawner] Clear is available only in Play Mode.", this);
                return;
            }

            ClearSpawned();
        }

        public void Spawn()
        {
            if (_hasSpawned || _enemyService == null)
            {
                return;
            }

            _spawnReservations.Clear();

            Transform parent = _spawnRoot != null ? _spawnRoot : transform;

            for (int spawnPointIndex = 0; spawnPointIndex < _spawnPoints.Count; spawnPointIndex++)
            {
                EnemySpawnPointData spawnPoint = _spawnPoints[spawnPointIndex];
                if (spawnPoint == null || spawnPoint.Radius <= 0f)
                {
                    continue;
                }

                SpawnConfiguredGroup(parent, spawnPoint, spawnPointIndex);
            }

            SpawnRandomEnemies(parent);
            _hasSpawned = true;
        }

        public void ClearSpawned()
        {
            for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
            {
                GameObject spawnedEnemy = _spawnedEnemies[i];
                if (spawnedEnemy == null)
                {
                    continue;
                }

                if (_gameObjectFactory != null)
                {
                    _gameObjectFactory.Destroy(spawnedEnemy);
                }
                else
                {
                    Destroy(spawnedEnemy);
                }
            }

            _spawnedEnemies.Clear();
            _spawnReservations.Clear();
            _hasSpawned = false;
        }

        public IReadOnlyList<EnemySpawnPointData> SpawnPoints => _spawnPoints;

        private void TryAutoSpawn()
        {
            if (_spawnOnStart == false || _hasSpawned)
            {
                return;
            }

            Spawn();
        }

        private void SpawnConfiguredGroup(Transform parent, EnemySpawnPointData spawnPoint, int spawnPointIndex)
        {
            if (spawnPoint.Enemies == null)
            {
                return;
            }

            for (int entryIndex = 0; entryIndex < spawnPoint.Enemies.Count; entryIndex++)
            {
                EnemySpawnEntry entry = spawnPoint.Enemies[entryIndex];
                if (entry?.Config == null)
                {
                    continue;
                }

                int spawnCount = Mathf.Max(1, entry.Count);
                for (int i = 0; i < spawnCount; i++)
                {
                    string spawnLabel = $"{spawnPointIndex}:{entryIndex}:{i}";
                    SpawnSingleEnemy(entry.Config, spawnPoint, parent, spawnLabel);
                }
            }
        }

        private void SpawnRandomEnemies(Transform parent)
        {
            if (_randomSpawnCount <= 0 || _randomEnemyConfigs == null || _randomEnemyConfigs.Count == 0)
            {
                return;
            }

            List<EnemyConfig> validRandomConfigs = BuildValidRandomConfigPool();
            if (validRandomConfigs.Count == 0)
            {
                return;
            }

            if (TryBuildNavMeshTriangleAreas(out List<NavMeshTriangleArea> triangleAreas, out float totalArea) == false)
            {
                Debug.LogWarning($"[EnemySpawner] Global random spawn is unavailable because NavMesh triangulation is empty for '{name}'.", this);
                return;
            }

            for (int i = 0; i < _randomSpawnCount; i++)
            {
                EnemyConfig config = validRandomConfigs[UnityEngine.Random.Range(0, validRandomConfigs.Count)];
                SpawnRandomEnemy(config, parent, triangleAreas, totalArea, $"random:{i}");
            }
        }

        private List<EnemyConfig> BuildValidRandomConfigPool()
        {
            List<EnemyConfig> validConfigs = new();
            for (int i = 0; i < _randomEnemyConfigs.Count; i++)
            {
                EnemyConfig config = _randomEnemyConfigs[i];
                if (config != null)
                {
                    validConfigs.Add(config);
                }
            }

            return validConfigs;
        }

        private void SpawnRandomEnemy(
            EnemyConfig config,
            Transform parent,
            IReadOnlyList<NavMeshTriangleArea> triangleAreas,
            float totalArea,
            string spawnLabel)
        {
            if (config == null)
            {
                return;
            }

            if (TryFindRandomNavMeshSpawnPosition(config, triangleAreas, totalArea, out Vector3 spawnPosition, out float spawnRadius) == false)
            {
                Debug.LogWarning($"[EnemySpawner] Failed to find global random spawn position for '{config.name}' in '{name}'.", this);
                return;
            }

            Quaternion spawnRotation = GetSpawnRotation(spawnPosition);
            EnemyAI enemy = _enemyService.Spawn(config, spawnPosition, spawnRotation, parent);
            if (enemy == null)
            {
                return;
            }

            enemy.name = $"{config.name}_{spawnLabel}";
            _spawnedEnemies.Add(enemy.gameObject);
            _spawnReservations.Add(new SpawnReservation(spawnPosition, spawnRadius));
        }

        private void SpawnSingleEnemy(EnemyConfig config, EnemySpawnPointData spawnPoint, Transform parent, string spawnLabel)
        {
            if (config == null)
            {
                return;
            }

            if (TryFindSpawnPosition(config, spawnPoint, out Vector3 spawnPosition, out float spawnRadius) == false)
            {
                Debug.LogWarning($"[EnemySpawner] Failed to find spawn position for '{config.name}' in '{name}'.", this);
                return;
            }

            Quaternion spawnRotation = GetSpawnRotation(spawnPosition);
            EnemyAI enemy = _enemyService.Spawn(config, spawnPosition, spawnRotation, parent);
            if (enemy == null)
            {
                return;
            }

            enemy.name = $"{config.name}_{spawnLabel}";
            _spawnedEnemies.Add(enemy.gameObject);
            _spawnReservations.Add(new SpawnReservation(spawnPosition, spawnRadius));
        }

        private bool TryFindSpawnPosition(EnemyConfig config, EnemySpawnPointData spawnPoint, out Vector3 spawnPosition, out float spawnRadius)
        {
            spawnPosition = default;
            spawnRadius = Mathf.Max(MinSpawnRadius, _enemyService.GetSpawnRadius(config) + _spawnSpacingPadding);
            if (spawnPoint == null)
            {
                return false;
            }

            Vector3 worldCenter = GetSpawnPointCenter(spawnPoint);
            float availableRadius = Mathf.Max(0f, spawnPoint.Radius - spawnRadius);
            int attempts = Mathf.Max(1, _spawnAttemptsPerEnemy);

            for (int i = 0; i < attempts; i++)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * availableRadius;
                Vector3 candidatePosition = worldCenter + new Vector3(randomOffset.x, 0f, randomOffset.y);

                if (TryResolveNavMeshPoint(candidatePosition, worldCenter, spawnPoint.Radius, spawnRadius, out Vector3 resolvedPosition) == false)
                {
                    continue;
                }

                if (IsPositionReserved(resolvedPosition, spawnRadius))
                {
                    continue;
                }

                spawnPosition = resolvedPosition;
                return true;
            }

            if (TryResolveNavMeshPoint(worldCenter, worldCenter, spawnPoint.Radius, spawnRadius, out Vector3 centerPosition)
                && IsPositionReserved(centerPosition, spawnRadius) == false)
            {
                spawnPosition = centerPosition;
                return true;
            }

            return false;
        }

        private bool TryFindRandomNavMeshSpawnPosition(
            EnemyConfig config,
            IReadOnlyList<NavMeshTriangleArea> triangleAreas,
            float totalArea,
            out Vector3 spawnPosition,
            out float spawnRadius)
        {
            spawnPosition = default;
            spawnRadius = Mathf.Max(MinSpawnRadius, _enemyService.GetSpawnRadius(config) + _spawnSpacingPadding);

            int attempts = Mathf.Max(1, _spawnAttemptsPerEnemy);
            for (int i = 0; i < attempts; i++)
            {
                if (TryGetRandomPointOnNavMesh(triangleAreas, totalArea, out Vector3 candidatePosition) == false)
                {
                    return false;
                }

                if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, Mathf.Max(_navMeshSampleDistance, spawnRadius), NavMesh.AllAreas) == false)
                {
                    continue;
                }

                if (IsPositionReserved(hit.position, spawnRadius))
                {
                    continue;
                }

                spawnPosition = hit.position;
                return true;
            }

            return false;
        }

        private bool TryResolveNavMeshPoint(
            Vector3 candidatePosition,
            Vector3 worldCenter,
            float areaRadius,
            float spawnRadius,
            out Vector3 resolvedPosition)
        {
            resolvedPosition = default;

            float sampleDistance = Mathf.Max(_navMeshSampleDistance, spawnRadius);
            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas) == false)
            {
                return false;
            }

            Vector3 planarOffset = hit.position - worldCenter;
            planarOffset.y = 0f;
            float maxRadius = Mathf.Max(0f, areaRadius - spawnRadius);
            if (planarOffset.sqrMagnitude > maxRadius * maxRadius + 0.01f)
            {
                return false;
            }

            resolvedPosition = hit.position;
            return true;
        }

        private bool IsPositionReserved(Vector3 position, float radius)
        {
            for (int i = 0; i < _spawnReservations.Count; i++)
            {
                SpawnReservation reservation = _spawnReservations[i];
                Vector3 delta = reservation.Position - position;
                delta.y = 0f;

                float minDistance = reservation.Radius + radius;
                if (delta.sqrMagnitude < minDistance * minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            DrawSpawnAreas();
        }

        private void OnDrawGizmosSelected()
        {
            DrawSpawnAreas();
        }

        private void DrawSpawnAreas()
        {
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                EnemySpawnPointData spawnPoint = _spawnPoints[i];
                if (spawnPoint == null || spawnPoint.Radius <= 0f)
                {
                    continue;
                }

                Vector3 worldCenter = GetSpawnPointCenter(spawnPoint);
                Gizmos.color = new Color(0.2f, 0.85f, 0.35f, 0.85f);
                Gizmos.DrawWireSphere(worldCenter, spawnPoint.Radius);
                Gizmos.DrawSphere(worldCenter, 0.12f);
                if (spawnPoint.Center != null)
                {
                    Gizmos.DrawLine(transform.position, spawnPoint.Center.position);
                }
            }

        }

        private readonly struct SpawnReservation
        {
            public SpawnReservation(Vector3 position, float radius)
            {
                Position = position;
                Radius = radius;
            }

            public Vector3 Position { get; }
            public float Radius { get; }
        }

        private Vector3 GetSpawnPointCenter(EnemySpawnPointData spawnPoint)
        {
            if (spawnPoint?.Center != null)
            {
                return spawnPoint.Center.position;
            }

            return transform.position;
        }

        private Quaternion GetSpawnRotation(Vector3 spawnPosition)
        {
            Transform playerTransform = _playerService?.PlayerTransform;
            if (playerTransform == null)
            {
                return transform.rotation;
            }

            Vector3 directionToPlayer = playerTransform.position - spawnPosition;
            directionToPlayer.y = 0f;
            if (directionToPlayer.sqrMagnitude < 0.0001f)
            {
                return transform.rotation;
            }

            return Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up);
        }

        private static bool TryBuildNavMeshTriangleAreas(out List<NavMeshTriangleArea> triangleAreas, out float totalArea)
        {
            triangleAreas = new List<NavMeshTriangleArea>();
            totalArea = 0f;

            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            Vector3[] vertices = triangulation.vertices;
            int[] indices = triangulation.indices;
            if (vertices == null || indices == null || indices.Length < 3)
            {
                return false;
            }

            for (int i = 0; i <= indices.Length - 3; i += 3)
            {
                Vector3 a = vertices[indices[i]];
                Vector3 b = vertices[indices[i + 1]];
                Vector3 c = vertices[indices[i + 2]];
                float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                if (area < MinTriangleArea)
                {
                    continue;
                }

                totalArea += area;
                triangleAreas.Add(new NavMeshTriangleArea(a, b, c, totalArea));
            }

            return triangleAreas.Count > 0 && totalArea > MinTriangleArea;
        }

        private static bool TryGetRandomPointOnNavMesh(
            IReadOnlyList<NavMeshTriangleArea> triangleAreas,
            float totalArea,
            out Vector3 randomPoint)
        {
            randomPoint = default;
            if (triangleAreas == null || triangleAreas.Count == 0 || totalArea <= MinTriangleArea)
            {
                return false;
            }

            float randomArea = UnityEngine.Random.Range(0f, totalArea);
            NavMeshTriangleArea selectedTriangle = triangleAreas[triangleAreas.Count - 1];
            for (int i = 0; i < triangleAreas.Count; i++)
            {
                if (randomArea <= triangleAreas[i].CumulativeArea)
                {
                    selectedTriangle = triangleAreas[i];
                    break;
                }
            }

            float r1 = Mathf.Sqrt(UnityEngine.Random.value);
            float r2 = UnityEngine.Random.value;
            randomPoint =
                (1f - r1) * selectedTriangle.A +
                (r1 * (1f - r2)) * selectedTriangle.B +
                (r1 * r2) * selectedTriangle.C;

            return true;
        }

        private readonly struct NavMeshTriangleArea
        {
            public NavMeshTriangleArea(Vector3 a, Vector3 b, Vector3 c, float cumulativeArea)
            {
                A = a;
                B = b;
                C = c;
                CumulativeArea = cumulativeArea;
            }

            public Vector3 A { get; }
            public Vector3 B { get; }
            public Vector3 C { get; }
            public float CumulativeArea { get; }
        }
    }

#if UNITY_EDITOR
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
#endif
}
