using System;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public Vector3SaveData Position;
        public Vector3SaveData Rotation;
        public Vector2SaveData CameraAngles;
        public float CurrentHealth;
        public float MaxHealth;
        public float MagicCooldownRemaining;
        public float MagicCooldownDuration;
    }
}
