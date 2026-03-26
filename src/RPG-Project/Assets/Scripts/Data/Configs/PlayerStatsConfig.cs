using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Configs/Players stats config")]
    public class PlayerStatsConfig : ScriptableObject
    {
        [Header("Base")]
        public float InitialHealth = 100;

        [Header("Attack stats")]
        public float PhysicalDamage = 25f;

        public float MagicDamage = 40f;
        public float MeleeHitRadius = 0.6f;
        public float MagicAttackCooldown = 2f;
        public float ProjectileSpeed = 25f;

        [Header("Movement stats")] 
        public float MoveSpeedCoef = 10f;
        public float WalkSpeed = 4f;
        public float RunSpeed = 6f;
        public float DownwardsMultiplier = 1f;
        public float UpwardsMultiplier = 1f;
        public float JumpVelocity = 2f;
        public float VelocityLimit = 5f;
    }
}