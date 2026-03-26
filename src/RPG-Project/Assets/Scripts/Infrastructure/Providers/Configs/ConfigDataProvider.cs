using System.Collections.Generic;
using System.Linq;
using Data.Configs;
using Data.Paths;
using Infrastructure.Services.UI;
using UnityEngine;
using Zenject;

namespace Infrastructure.Providers.Configs
{
    public class ConfigDataProvider : IConfigDataProvider, IInitializable
    {
        private WindowsConfig _windowsConfig;
        private PlayerStatsConfig _playerStatsConfig;
        private Dictionary<int, EnemyConfig> _enemies;
        public void Initialize() => Load();
        public void Load()
        {
            _windowsConfig = Resources.LoadAll<WindowsConfig>(ConfigPaths.WINDOWS_CONFIG_PATH).FirstOrDefault();
            
            _playerStatsConfig = Resources.LoadAll<PlayerStatsConfig>(ConfigPaths.PLAYERSTATS_CONFIG_PATH).FirstOrDefault();
            
            _enemies = Resources.LoadAll<EnemyConfig>(ConfigPaths.ENEMIES_CONFIG_PATH)
                .ToDictionary(x => x.Id, x => x);
            
            Debug.Log($"[ConfigDataProvider] Loaded {_windowsConfig.windows.Count} UI windows.");
            Debug.Log($"[ConfigDataProvider] Loaded {_enemies.Values.Count} enemies.");
        }

        public GameObject GetWindowPrefab(WindowID id)
        {
            var record = _windowsConfig.windows.FirstOrDefault(x => x.windowID == id);
            return record.prefab;
        }

        public PlayerStatsConfig GetPlayerStatsConfig() => _playerStatsConfig;
        public EnemyConfig GetEnemyConfig(int id) => _enemies.TryGetValue(id, out var config) ? config : null;
    }
}