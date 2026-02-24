using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Configs/Players stats config")]
    public class PlayerStatsConfig : ScriptableObject
    {
        [Header("Base")]
        public float InitialHealth = 100;

        [Header("Attack stats")]
        public float MagicAttackCooldown = 2f;

        [Header("Movement stats")]
        public float MoveSpeed = 10f;
        public float DownwardsMultiplier = 1f;
        public float UpwardsMultiplier = 1f;
        public float JumpVelocity = 2f;
        public float VelocityLimit = 5f;
    }
}