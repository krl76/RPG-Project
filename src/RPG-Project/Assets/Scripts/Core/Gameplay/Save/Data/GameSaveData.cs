using System;
using System.Collections.Generic;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    /// <summary>
    /// Корневой объект сохранения игрового сеанса.
    /// </summary>
    public sealed class GameSaveData
    {
        public int Version = 2;
        public string SavedAtUtc;
        public PlayerSaveData Player;
        public List<EnemySaveData> Enemies = new List<EnemySaveData>();
        public GameplayProgressSaveData Progress;
    }
}
