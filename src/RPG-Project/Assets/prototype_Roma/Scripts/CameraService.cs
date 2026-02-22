using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace prototype_Roma.Scripts
{
    public class CameraService : ICameraService
    {
        public event Action CameraRotationChanged;
        public GameObject CameraObject { get; private set; }
        public Camera Camera { get; private set; }
        
        private Vector2 _standardCameraRotation = Vector2.zero;
        
        private CinemachineCamera _cinemachineCamera;
        private CinemachinePositionComposer _composer;
        private CinemachinePanTilt _panTilt;

        private readonly IPlayerService _playerService;

        public CameraService(IPlayerService playerService)
        {
            _playerService = playerService;
        }
        
        public void InstallService()
        {
            _cinemachineCamera = Object.FindAnyObjectByType<CinemachineCamera>();
            Camera = Camera.main;
            CameraObject = cinemachineCamera.gameObject;
            _composer = _cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            _panTilt = _cinemachineCamera.GetComponent<CinemachinePanTilt>();
            _cinemachineCamera.LookAt = _playerService.player.transform;
            _cinemachineCamera.Follow = _playerService.player.transform;
            _standardCameraRotation = new Vector2(_panTilt.PanAxis.Value, _panTilt.TiltAxis.Value);

            // install mouse input
        }

        public void UninstallService()
        {
            // uninstall mouse input
        }

        public void ChangeDistance(float newDistance)
        {
            _composer.CameraDistance = newDistance;
            
            if (_cinemachineCamera.Lens.Orthographic)
            {
                _cinemachineCamera.Lens.OrthographicSize = newDistance;
            }
        }

        public float GetPanAxisRotation()
        {
            return _panTilt.PanAxis.Value;
        }

        private void MoveMouse(InputAction.CallbackContext context)
        {
            Vector2 rotationChange = Vector2.zero; // get from mouse
            
            _panTilt.PanAxis.Value = _standardCameraRotation.x + rotationChange.x; // Vertical Axis (Pitch)
            _panTilt.TiltAxis.Value = _standardCameraRotation.y + rotationChange.y; // Horizontal Axis (Yaw)
            CameraRotationChanged?.Invoke(); // проверка на изменение
        }
    }
}