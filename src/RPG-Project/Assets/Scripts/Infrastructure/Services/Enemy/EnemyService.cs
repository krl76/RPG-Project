using System.Collections.Generic;
using System.Linq;
using Core.Gameplay.Save.Data;
using Data.Configs;
using Features.Enemy;
using Infrastructure.Factories.Objects;
using UnityEngine;
using UnityEngine.AI;

namespace Infrastructure.Services.Enemy
{
    /// <summary>
    /// Создаёт врагов, хранит их runtime-состояние и данные для save/load.
    /// </summary>
    public class EnemyService : IEnemyService
    {
        private readonly IGameObjectFactory _gameObjectFactory;
        private readonly List<EnemyAI> _enemies = new List<EnemyAI>();
        private readonly Dictionary<string, EnemySaveData> _trackedStates = new Dictionary<string, EnemySaveData>();

        public EnemyService(IGameObjectFactory gameObjectFactory)
        {
            _gameObjectFactory = gameObjectFactory;
        }

        public IReadOnlyList<EnemyAI> ActiveEnemies => _enemies;

        public EnemyAI Spawn(EnemyConfig config, Vector3 position, Quaternion rotation, Transform parent = null, string saveId = null)
        {
            if (TryResolvePrefab(config, out GameObject enemyPrefab) == false)
            {
                return null;
            }

            GameObject enemyObject = _gameObjectFactory.Instantiate(enemyPrefab, position, rotation, parent);
            enemyObject.transform.SetPositionAndRotation(position, rotation);
            if (enemyObject.TryGetComponent(out EnemyAI enemyAI))
            {
                enemyAI.SetSaveId(saveId);
                return enemyAI;
            }

            Debug.LogError($"[EnemyService] Prefab '{enemyPrefab.name}' does not contain {nameof(EnemyAI)}.", enemyObject);
            _gameObjectFactory.Destroy(enemyObject);
            return null;
        }

        public float GetSpawnRadius(EnemyConfig config)
        {
            if (TryResolvePrefab(config, out GameObject enemyPrefab) == false)
            {
                return 0.5f;
            }

            float radius = 0.5f;

            if (enemyPrefab.TryGetComponent(out NavMeshAgent navMeshAgent))
            {
                radius = Mathf.Max(radius, navMeshAgent.radius);
            }

            if (enemyPrefab.TryGetComponent(out CapsuleCollider capsuleCollider))
            {
                radius = Mathf.Max(radius, capsuleCollider.radius);
            }

            if (enemyPrefab.TryGetComponent(out SphereCollider sphereCollider))
            {
                radius = Mathf.Max(radius, sphereCollider.radius);
            }

            if (enemyPrefab.TryGetComponent(out CharacterController characterController))
            {
                radius = Mathf.Max(radius, characterController.radius);
            }

            return radius;
        }

        public void Register(EnemyAI enemy)
        {
            if (!_enemies.Contains(enemy))
            {
                _enemies.Add(enemy);
            }

            if (enemy != null)
            {
                _trackedStates[enemy.SaveId] = enemy.CaptureSaveData();
            }
        }

        public void Unregister(EnemyAI enemy)
        {
            if (_enemies.Contains(enemy))
            {
                _enemies.Remove(enemy);
            }
        }

        public void MarkDead(EnemyAI enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var snapshot = enemy.CaptureSaveData();
            snapshot.IsAlive = false;
            snapshot.CurrentHealth = 0f;
            _trackedStates[enemy.SaveId] = snapshot;
        }

        public void RestoreSavedState(EnemySaveData enemySaveData)
        {
            if (enemySaveData == null || string.IsNullOrWhiteSpace(enemySaveData.Id))
            {
                return;
            }

            _trackedStates[enemySaveData.Id] = Clone(enemySaveData);
        }

        public IReadOnlyList<EnemySaveData> CaptureSaveData()
        {
            var combinedState = new Dictionary<string, EnemySaveData>(_trackedStates);

            foreach (EnemyAI enemy in _enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                combinedState[enemy.SaveId] = enemy.CaptureSaveData();
            }

            return combinedState.Values.ToList();
        }

        public void ResetRuntimeData()
        {
            _enemies.Clear();
            _trackedStates.Clear();
        }

        private static bool TryResolvePrefab(EnemyConfig config, out GameObject enemyPrefab)
        {
            enemyPrefab = config != null ? config.EnemyPrefab : null;
            if (enemyPrefab != null)
            {
                return true;
            }

            Debug.LogWarning($"[EnemyService] Enemy prefab is not assigned for config '{config?.name ?? "null"}'.");
            return false;
        }

        private static EnemySaveData Clone(EnemySaveData data) =>
            new EnemySaveData
            {
                Id = data.Id,
                ConfigId = data.ConfigId,
                IsAlive = data.IsAlive,
                IsProvoked = data.IsProvoked,
                IsEnraged = data.IsEnraged,
                HasSelectedRegularVariation = data.HasSelectedRegularVariation,
                SelectedRegularVariationIndex = data.SelectedRegularVariationIndex,
                HasSelectedBossElement = data.HasSelectedBossElement,
                SelectedBossElementIndex = data.SelectedBossElementIndex,
                CurrentHealth = data.CurrentHealth,
                MaxHealth = data.MaxHealth,
                RuntimeStateId = data.RuntimeStateId,
                Position = data.Position,
                Rotation = data.Rotation
            };
    }
}
