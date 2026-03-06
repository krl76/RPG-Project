using System;
using UnityEngine;

namespace Data.SaveData
{
    [Serializable]
    public class BossData
    {
        public int health;
        public int weaponType;
        public int elementType;

        public Vector3 position;
        public Quaternion rotation;
        
        public BossData(int newHealth, int newWeaponType, int newElementType, Vector3 newPosition, Quaternion newRotation)
        {
            health = newHealth;
            weaponType = newWeaponType;
            elementType = newElementType;
            position = newPosition;
            rotation = newRotation;
        }
    }
}