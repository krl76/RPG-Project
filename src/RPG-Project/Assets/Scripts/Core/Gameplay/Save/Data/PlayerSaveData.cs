using System;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    /// <summary>
    /// Снимок состояния игрока для сохранения.
    /// </summary>
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
