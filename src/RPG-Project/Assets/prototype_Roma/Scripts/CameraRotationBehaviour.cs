using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace prototype_Roma.Scripts
{
    public class CameraRotationBehaviour : MonoBehaviour
    {
        private ICameraService _cameraService;

        private Camera _currentCamera;

        private float _angle = 0;
        
        [Inject]
        private void Construct(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        private void Start()
        {
            _currentCamera = _cameraService.Camera;
        }

        private void Update()
        {
            Vector3 mouseScreenPositon = _currentCamera.ScreenToViewportPoint(Mouse.current.position.ReadValue());
            if (mouseScreenPositon.x < 0.3) _angle -= (0.3f - mouseScreenPositon.x) * 300 * Time.deltaTime;
            else if (mouseScreenPositon.x > 0.7) _angle += (mouseScreenPositon.x - 0.7f) * 300 * Time.deltaTime;
            else return;
            _cameraService.SetRotationAngle(_angle);
        }
    }
}