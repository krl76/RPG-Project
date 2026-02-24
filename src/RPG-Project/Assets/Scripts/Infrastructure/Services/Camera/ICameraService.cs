using System;
using UnityEngine;

namespace Infrastructure.Services.Camera
{
    public interface ICameraService
    {
        public event Action CameraRotationChanged;
        public GameObject CameraObject { get; }
        public UnityEngine.Camera Camera { get; }
        public void InstallService();
        public void ChangeDistance(float newDistance);
        public void SetRotationAngle(float newAngle);
        public Quaternion GetCameraRotation();
        public float GetCameraAngle();
    }
}