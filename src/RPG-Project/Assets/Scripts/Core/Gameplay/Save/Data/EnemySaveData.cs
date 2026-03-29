using System;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    public sealed class EnemySaveData
    {
        public string Id;
        public bool IsAlive;
        public bool IsProvoked;
        public bool IsEnraged;
        public bool HasSelectedRegularVariation;
        public int SelectedRegularVariationIndex = -1;
        public bool HasSelectedBossElement;
        public int SelectedBossElementIndex = -1;
        public float CurrentHealth;
        public float MaxHealth;
        public string RuntimeStateId;
        public Vector3SaveData Position;
        public Vector3SaveData Rotation;
    }
}
