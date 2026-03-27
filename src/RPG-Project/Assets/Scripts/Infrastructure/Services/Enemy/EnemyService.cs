using System.Collections.Generic;
using System.Linq;
using Core.Gameplay.Save.Data;
using Features.Enemy;

namespace Infrastructure.Services.Enemy
{
    public class EnemyService : IEnemyService
    {
        private readonly List<EnemyAI> _enemies = new List<EnemyAI>();
        private readonly Dictionary<string, EnemySaveData> _trackedStates = new Dictionary<string, EnemySaveData>();

        public IReadOnlyList<EnemyAI> ActiveEnemies => _enemies;

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
    }
}
