using Data.Configs;
using Infrastructure.Services.UI;
using UnityEngine;

namespace Infrastructure.Providers.Configs
{
    /// <summary>
    /// Контракт доступа к игровым конфигам и UI-префабам.
    /// </summary>
    public interface IConfigDataProvider
    {
        void Load();
        GameObject GetWindowPrefab(WindowID id);
        GameConfig GetGameConfig();
        PlayerStatsConfig GetPlayerStatsConfig();
        EnemyConfig GetEnemyConfig(int id);
        CombatAudioConfig GetCombatAudioConfig();
    }
}
