using System;
using UnityEngine;

namespace Data.SaveData
{
    [Serializable]
    public class PlayerData
    {
        public int health;
        public int mana;
        public float magicAttackCooldown;

        public Vector3 position;
        public Quaternion rotation;

        public PlayerData(int newHealth = 100, int newMana = 100, float magicCD = 0)
        {
            health = newHealth;
            mana = newMana;
            magicAttackCooldown = magicCD;
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        public PlayerData(Vector3 newPosition, Quaternion newRotation, int newHealth = 100,
            int newMana = 100, float magicCD = 0)
        {
            health = newHealth;
            mana = newMana;
            magicAttackCooldown = magicCD;
            position = newPosition;
            rotation = newRotation;
        }
    }
}