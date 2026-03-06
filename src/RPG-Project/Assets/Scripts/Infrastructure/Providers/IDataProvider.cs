using System.Collections.Generic;
using Data.SaveData;
using UnityEngine;

namespace Infrastructure.Providers
{
    public interface IDataProvider
    { 
        public bool CanBeLoaded { get; }
        public void LoadData();
        public void SaveData();
        public void DeleteSave();
        public void SaveEnemyData(List<EnemyData> enemies, List<BossData> bosses);
        public void SavePlayerPosition(Vector3 postion, Quaternion rotation);
        public void SavePlayerCombatData(int health, int mana);
        public void SavePlayerMagicCooldown(float cooldown);
        public void SaveLevelData(int score, int killsForBoss, int killsForWin, bool isPeaceful);
        public PlayerData GetPlayerData();
        public LevelData GetLevelData();
    }
}