using System;
using System.Collections.Generic;

namespace Data.SaveData
{
    [Serializable]
    public class LevelData
    {
        public int score;
        public int killsForBoss; // how many enemies remain for boss to spawn
        public int killsForWin; // how many enemies remain for win melody

        public bool isPeaceful;
        
        public List<EnemyData> enemiesOnLevel;
        public List<BossData> bossesOnLevel;

        public LevelData()
        {
            score = 0;
            killsForBoss = 3;
            killsForWin = 5;
            isPeaceful = false;
            enemiesOnLevel = new List<EnemyData>();
            bossesOnLevel = new List<BossData>();
        }
    }
}