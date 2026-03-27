using System.Collections.Generic;
using Core.Gameplay.Save.Data;
using Features.Enemy;

namespace Infrastructure.Services.Enemy
{
    public interface IEnemyService
    {
        void Register(EnemyAI enemy);
        void Unregister(EnemyAI enemy);
        void MarkDead(EnemyAI enemy);
        IReadOnlyList<EnemySaveData> CaptureSaveData();
        void ResetRuntimeData();
        IReadOnlyList<EnemyAI> ActiveEnemies { get; }
    }
}
