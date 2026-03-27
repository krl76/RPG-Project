using System;
using UnityEngine;

namespace Core.Gameplay.Save.Data
{
    [Serializable]
    public struct Vector3SaveData
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3 ToVector3() => new Vector3(X, Y, Z);

        public static Vector3SaveData FromVector3(Vector3 value) =>
            new Vector3SaveData
            {
                X = value.x,
                Y = value.y,
                Z = value.z
            };
    }
}
