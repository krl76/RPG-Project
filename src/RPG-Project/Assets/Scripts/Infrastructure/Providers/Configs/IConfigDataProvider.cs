using Data.Configs;
using Infrastructure.Services.UI;
using UnityEngine;

namespace Infrastructure.Providers.Configs
{
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
