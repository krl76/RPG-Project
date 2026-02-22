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
        public void UninstallService();
        public void ChangeDistance(float newDistance);
        public float GetPanAxisRotation();
    }
}