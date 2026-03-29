using Infrastructure.Factories.Objects;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Features.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform _spawnRoot;
        [SerializeField, Min(1)] private int _spawnCount = 1;
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField] private bool _useRandomSpawnPoints = true;
        [SerializeField] private List<Transform> _spawnPoints = new();

        private readonly List<GameObject> _spawnedEnemies = new();

        private IGameObjectFactory _gameObjectFactory;
        private bool _hasSpawned;

        [Inject]
        private void Construct(IGameObjectFactory gameObjectFactory)
        {
            _gameObjectFactory = gameObjectFactory;
        }

        private void Awake()
        {
            TryAutoSpawn();
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
            if (_hasSpawned || _gameObjectFactory == null)
            {
                return;
            }

            if (_enemyPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] Enemy prefab is not assigned.", this);
                return;
            }

            Transform parent = _spawnRoot != null ? _spawnRoot : transform;
            List<Transform> orderedSpawnPoints = BuildOrderedSpawnPoints();
            int spawnCount = Mathf.Max(1, _spawnCount);

            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = ResolveSpawnPoint(i, orderedSpawnPoints);
                Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
                Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

                GameObject enemy = _gameObjectFactory.Instantiate(_enemyPrefab, position, rotation, parent);
                enemy.name = $"SpawnedEnemy_{i:00}";
                _spawnedEnemies.Add(enemy);
            }

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

                _gameObjectFactory?.Destroy(spawnedEnemy);
            }

            _spawnedEnemies.Clear();
            _hasSpawned = false;
        }

        private void TryAutoSpawn()
        {
            if (_spawnOnStart == false || _hasSpawned)
            {
                return;
            }

            Spawn();
        }

        private List<Transform> BuildOrderedSpawnPoints()
        {
            List<Transform> validPoints = new();
            foreach (Transform spawnPoint in _spawnPoints)
            {
                if (spawnPoint != null)
                {
                    validPoints.Add(spawnPoint);
                }
            }

            if (_useRandomSpawnPoints == false)
            {
                return validPoints;
            }

            for (int i = validPoints.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                Transform temp = validPoints[i];
                validPoints[i] = validPoints[swapIndex];
                validPoints[swapIndex] = temp;
            }

            return validPoints;
        }

        private Transform ResolveSpawnPoint(int index, IReadOnlyList<Transform> orderedSpawnPoints)
        {
            if (orderedSpawnPoints == null || orderedSpawnPoints.Count == 0)
            {
                return null;
            }

            if (_useRandomSpawnPoints)
            {
                return orderedSpawnPoints[index % orderedSpawnPoints.Count];
            }

            return orderedSpawnPoints[Mathf.Clamp(index, 0, orderedSpawnPoints.Count - 1)];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.35f, 0.8f, 0.35f, 0.85f);

            if (_spawnPoints.Count == 0)
            {
                Gizmos.DrawWireSphere(transform.position, 0.35f);
                return;
            }

            foreach (Transform spawnPoint in _spawnPoints)
            {
                if (spawnPoint == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(spawnPoint.position, 0.35f);
                Gizmos.DrawLine(transform.position, spawnPoint.position);
            }
        }
    }
}
