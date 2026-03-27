using System;
using UnityEngine;

namespace Infrastructure.Services.Camera
{
    public interface ICameraService
    {
        event Action CameraRotationChanged;
        GameObject CameraObject { get; }
        UnityEngine.Camera Camera { get; }
        void InstallService();
        void ChangeDistance(float newDistance);
        void SetCameraAngle(Vector2 rotation);
        void SetRotationAngle(Vector2 deltaRotation); 
        Quaternion GetCameraRotation();
        Vector2 GetCameraAngle();
    }
}
