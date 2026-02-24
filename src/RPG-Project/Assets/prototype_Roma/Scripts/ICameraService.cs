using System;
using UnityEngine;

namespace prototype_Roma.Scripts
{
    public interface ICameraService
    {
        public event Action CameraRotationChanged;
        public GameObject CameraObject { get; }
        public Camera Camera { get; }
        public void InstallService();
        public void ChangeDistance(float newDistance);
        public void SetRotationAngle(float newAngle);
        public Quaternion GetCameraRotation();
        public float GetCameraAngle();
    }
}