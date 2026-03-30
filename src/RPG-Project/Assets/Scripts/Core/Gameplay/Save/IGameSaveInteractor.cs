using System.Collections.Generic;
using Core.Gameplay.Save.Data;

namespace Core.Gameplay.Save
{
    /// <summary>
    /// Координирует сохранение игры и подготовку данных к восстановлению.
    /// </summary>
    public interface IGameSaveInteractor
    {
        bool SaveGame();
        bool PrepareLoadGame();
        bool HasPendingRestore();
        IReadOnlyList<EnemySaveData> GetPendingEnemyStates();
        void ApplyPendingGameState();
        void ClearPendingRestore();
        bool HasSave();
    }
}
