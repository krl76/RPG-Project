using System;
using UnityEngine;

namespace Data.SaveData
{
    [Serializable]
    public class EnemyData
    {
        public int health;
        public EnemyType type;
        public int weaponType; // 0 or 1 for different weapons

        public Vector3 position;
        public Quaternion rotation;

        public EnemyData(int newHealth, EnemyType enemyType, int newWeaponType, Vector3 newPosition, Quaternion newRotation)
        {
            health = newHealth;
            type = enemyType;
            weaponType = newWeaponType;
            position = newPosition;
            rotation = newRotation;
        }
    }

    [Serializable]
    public enum EnemyType
    {
        Melee,
        Range
    }
}