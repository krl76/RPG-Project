using System;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    /// <summary>
    /// Снимок прогрессии боя: очки, убийства и триггеры событий.
    /// </summary>
    public sealed class GameplayProgressSaveData
    {
        public int CurrentScore;
        public int RegularEnemiesKilled;
        public bool BossSpawnTriggered;
        public bool VictoryTriggered;
    }
}
