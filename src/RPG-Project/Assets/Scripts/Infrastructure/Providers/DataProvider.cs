using System.Collections.Generic;
using System.IO;
using Data.SaveData;
using UnityEngine;
using Zenject;

namespace Infrastructure.Providers
{
    public class DataProvider : IInitializable, IDataProvider
    {
        public bool CanBeLoaded { get; private set; }
        
        private PlayerData _playerData;
        private LevelData _levelData;

        private string _savePath;

        public void Initialize()
        {
            _playerData = new PlayerData();
            _levelData = new LevelData();
            _savePath = Path.Combine(Application.persistentDataPath, "Save");
            Directory.CreateDirectory(_savePath);
            Debug.Log(_savePath);
            string path = Path.Combine(_savePath, "PlayerSave.json");
            CanBeLoaded = File.Exists(path);
        }

        public void LoadData()
        {
            string path;
            string jsonString;
            if (CanBeLoaded)
            {
                path = Path.Combine(_savePath, "PlayerSave.json");
                jsonString = File.ReadAllText(path);
                _playerData = JsonUtility.FromJson<PlayerData>(jsonString);
                path = Path.Combine(_savePath, "LevelSave.json");
                jsonString = File.ReadAllText(path);
                _levelData = JsonUtility.FromJson<LevelData>(jsonString);
            }
            else
            {
                _playerData = new PlayerData();
                _levelData = new LevelData();
            }
        }

        public void SaveData()
        {
            string path = Path.Combine(_savePath, "PlayerSave.json");
            string jsonString = JsonUtility.ToJson(_playerData);
            File.WriteAllText(path, jsonString);
            path = Path.Combine(_savePath, "LevelSave.json");
            jsonString = JsonUtility.ToJson(_levelData);
            File.WriteAllText(path, jsonString);
            CanBeLoaded = true;
        }

        public void DeleteSave()
        {
            _playerData = new PlayerData();
            _levelData = new LevelData();
            string path = Path.Combine(_savePath, "PlayerSave.json");
            string jsonString = JsonUtility.ToJson(_playerData);
            File.WriteAllText(path, jsonString);
            
            path = Path.Combine(_savePath, "LevelSave.json");
            jsonString = JsonUtility.ToJson(_levelData);
            File.WriteAllText(path, jsonString);
        }

        public void SaveEnemyData(List<EnemyData> enemies, List<BossData> bosses)
        {
            _levelData.bossesOnLevel = bosses;
            _levelData.enemiesOnLevel = enemies;
        }

        public void SavePlayerPosition(Vector3 postion, Quaternion rotation)
        {
            _playerData.position = postion;
            _playerData.rotation = rotation;
        }

        public void SavePlayerCombatData(int health, int mana)
        {
            _playerData.health = health;
            _playerData.mana = mana;
        }

        public void SavePlayerMagicCooldown(float cooldown) => _playerData.magicAttackCooldown = cooldown;

        public void SaveLevelData(int score, int killsForBoss, int killsForWin, bool isPeaceful)
        {
            _levelData.score = score;
            _levelData.killsForBoss = killsForBoss;
            _levelData.killsForWin = killsForBoss;
            _levelData.isPeaceful = isPeaceful;
        }

        public PlayerData GetPlayerData() => _playerData;

        public LevelData GetLevelData() => _levelData;
    }
}