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

        public void Load()
        {
            _windowsConfig = Resources.Load<WindowsConfig>(ConfigPaths.WINDOWS_CONFIG_PATH);
            if (_windowsConfig == null) Debug.LogError("[ConfigDataProvider] WindowsConfig not found!");

            Debug.Log($"[ConfigDataProvider] Loaded {_windowsConfig.windows.Count} UI windows.");
        }

        public GameObject GetWindowPrefab(WindowID id)
        {
            var record = _windowsConfig.windows.FirstOrDefault(x => x.windowID == id);
            return record.prefab;
        }
    }
}