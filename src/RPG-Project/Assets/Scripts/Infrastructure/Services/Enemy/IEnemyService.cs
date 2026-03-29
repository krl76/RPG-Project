using System.Collections.Generic;
using Core.Gameplay.Save.Data;
using Data.Configs;
using Features.Enemy;
using UnityEngine;

namespace Infrastructure.Services.Enemy
{
    public interface IEnemyService
    {
        EnemyAI Spawn(EnemyConfig config, Vector3 position, Quaternion rotation, Transform parent = null);
        float GetSpawnRadius(EnemyConfig config);
        void Register(EnemyAI enemy);
        void Unregister(EnemyAI enemy);
        void MarkDead(EnemyAI enemy);
        IReadOnlyList<EnemySaveData> CaptureSaveData();
        void ResetRuntimeData();
        IReadOnlyList<EnemyAI> ActiveEnemies { get; }
    }
}
