using System.Collections.Generic;
using Features.Enemy;

namespace Infrastructure.Services.Enemy
{
    public interface IEnemyService
    {
        void Register(EnemyAI enemy);
        void Unregister(EnemyAI enemy);
        IReadOnlyList<EnemyAI> ActiveEnemies { get; }
    }
}