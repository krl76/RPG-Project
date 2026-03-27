using System;
using UnityEngine;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    public struct Vector2SaveData
    {
        public float X;
        public float Y;

        public Vector2 ToVector2() => new Vector2(X, Y);

        public static Vector2SaveData FromVector2(Vector2 value) =>
            new Vector2SaveData
            {
                X = value.x,
                Y = value.y
            };
    }
}
