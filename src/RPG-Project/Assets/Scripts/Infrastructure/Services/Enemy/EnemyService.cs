using System.Collections.Generic;
using Features.Enemy;

namespace Infrastructure.Services.Enemy
{
    public class EnemyService : IEnemyService
    {
        private readonly List<EnemyAI> _enemies = new List<EnemyAI>();

        public IReadOnlyList<EnemyAI> ActiveEnemies => _enemies;

        public void Register(EnemyAI enemy)
        {
            if (!_enemies.Contains(enemy))
            {
                _enemies.Add(enemy);
            }
        }

        public void Unregister(EnemyAI enemy)
        {
            if (_enemies.Contains(enemy))
            {
                _enemies.Remove(enemy);
            }
        }
    }
}