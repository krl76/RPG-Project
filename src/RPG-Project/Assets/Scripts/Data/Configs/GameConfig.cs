using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Enemy Behaviour")]
        public bool PeacefulModeEnabled;
    }
}
