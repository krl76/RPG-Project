using System.Collections.Generic;
using Core.Gameplay.Save.Data;
using Data.Configs;
using Features.Enemy;
using UnityEngine;

namespace Infrastructure.Services.Enemy
{
    /// <summary>
    /// Контракт спавна, учёта и сохранения врагов.
    /// </summary>
    public interface IEnemyService
    {
        EnemyAI Spawn(EnemyConfig config, Vector3 position, Quaternion rotation, Transform parent = null, string saveId = null);
        float GetSpawnRadius(EnemyConfig config);
        void Register(EnemyAI enemy);
        void Unregister(EnemyAI enemy);
        void MarkDead(EnemyAI enemy);
        void RestoreSavedState(EnemySaveData enemySaveData);
        IReadOnlyList<EnemySaveData> CaptureSaveData();
        void ResetRuntimeData();
        IReadOnlyList<EnemyAI> ActiveEnemies { get; }
    }
}
