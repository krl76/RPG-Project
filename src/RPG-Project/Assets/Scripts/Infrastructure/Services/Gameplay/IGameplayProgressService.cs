using Core.Gameplay.Save.Data;
using Data.Configs;

namespace Infrastructure.Services.Gameplay
{
    /// <summary>
    /// Контракт сервиса очков и триггеров боевой прогрессии.
    /// </summary>
    public interface IGameplayProgressService
    {
        int CurrentScore { get; }
        int RegularEnemiesKilled { get; }
        bool IsBossSpawnTriggered { get; }

        void RegisterEnemyKill(EnemyConfig enemyConfig);
        GameplayProgressSaveData CaptureSaveData();
        void RestoreProgress(GameplayProgressSaveData data);
        void ResetRuntimeData();
    }
}
