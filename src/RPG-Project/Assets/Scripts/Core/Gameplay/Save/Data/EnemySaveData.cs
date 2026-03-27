using System;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    public sealed class EnemySaveData
    {
        public string Id;
        public bool IsAlive;
        public float CurrentHealth;
        public float MaxHealth;
        public Vector3SaveData Position;
        public Vector3SaveData Rotation;
    }
}
