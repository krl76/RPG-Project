using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
    /// <summary>
    /// Глобальный конфиг игровых правил и прогрессии.
    /// </summary>
    public class GameConfig : ScriptableObject
    {
        [Header("Enemy Behaviour")]
        public bool PeacefulModeEnabled;

        [Header("Progression")]
        [FormerlySerializedAs("ScoreRequiredForBossSpawn")]
        [Min(0)] public int RegularEnemyKillsRequiredForBossSpawn = 3;
        [Min(0)] public int RegularEnemyKillsRequiredForVictory = 5;
    }
}
