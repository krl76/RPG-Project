using System;
using System.Collections.Generic;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int Version = 1;
        public string SavedAtUtc;
        public PlayerSaveData Player;
        public List<EnemySaveData> Enemies = new List<EnemySaveData>();
    }
}
