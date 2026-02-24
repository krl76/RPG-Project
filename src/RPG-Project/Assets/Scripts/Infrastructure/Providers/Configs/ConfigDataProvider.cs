using System.Linq;
using Data.Configs;
using Data.Paths;
using Infrastructure.Services.UI;
using UnityEngine;

namespace Infrastructure.Providers.Configs
{
    public class ConfigDataProvider : IConfigDataProvider
    {
        private WindowsConfig _windowsConfig;
        private PlayerStatsConfig _playerStatsConfig;

        public void Load()
        {
            _windowsConfig = Resources.LoadAll<WindowsConfig>(ConfigPaths.WINDOWS_CONFIG_PATH).FirstOrDefault();
            if (_windowsConfig == null) Debug.LogError("[ConfigDataProvider] WindowsConfig not found!");
            
            _playerStatsConfig = Resources.LoadAll<PlayerStatsConfig>(ConfigPaths.PLAYERSTATS_CONFIG_PATH).FirstOrDefault();
            if (_playerStatsConfig == null) Debug.LogError("[ConfigDataProvider] PlayerStatsConfig not found!");

            Debug.Log($"[ConfigDataProvider] Loaded {_windowsConfig.windows.Count} UI windows.");
        }

        public GameObject GetWindowPrefab(WindowID id)
        {
            var record = _windowsConfig.windows.FirstOrDefault(x => x.windowID == id);
            return record.prefab;
        }

        public PlayerStatsConfig GetPlayerStatsConfig() => _playerStatsConfig;
    }
}