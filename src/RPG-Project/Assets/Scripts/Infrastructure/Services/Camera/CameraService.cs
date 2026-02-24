using System;
using Infrastructure.Services.Player;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Camera
{
    public class CameraService : ICameraService
    {
        public event Action CameraRotationChanged;
        public GameObject CameraObject { get; private set; }
        public UnityEngine.Camera Camera { get; private set; }

        private Quaternion _currentRotation;
        private float _currentAngle;
        
        private CinemachineCamera _cinemachineCamera;
        private CinemachinePositionComposer _composer;

        private readonly IPlayerService _playerService;

        public CameraService(IPlayerService playerService)
        {
            _playerService = playerService;
        }
        
        public void InstallService()
        {
            _cinemachineCamera = Object.FindAnyObjectByType<CinemachineCamera>();
            Camera = UnityEngine.Camera.main;
            CameraObject = _cinemachineCamera.gameObject;
            _composer = _cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            _cinemachineCamera.LookAt = _playerService.PlayerTransform;
            _cinemachineCamera.Follow = _playerService.PlayerTransform;
        }

        public void ChangeDistance(float newDistance)
        {
            _composer.CameraDistance = newDistance;
            
            if (_cinemachineCamera.Lens.Orthographic)
            {
                _cinemachineCamera.Lens.OrthographicSize = newDistance;
            }
        }

        public void SetRotationAngle(float newAngle)
        {
            if (Mathf.Approximately(newAngle, _currentAngle)) return;
            _currentAngle = newAngle;
            _currentRotation = Quaternion.Euler(0, newAngle, 0);
            CameraRotationChanged?.Invoke();
        }

        public Quaternion GetCameraRotation() => _currentRotation;
        public float GetCameraAngle() => _currentAngle;
    }
}