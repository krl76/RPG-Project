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

        private Transform _cameraTarget;
        private Vector2 _currentRotation;

        private CinemachineCamera _cinemachineCamera;
        private CinemachineThirdPersonFollow _thirdPersonFollow;

        private readonly IPlayerService _playerService;

        public CameraService(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public void InstallService()
        {
            if (_playerService.PlayerTransform == null)
            {
                Debug.LogError("[CameraService] PlayerTransform is not initialized.");
                return;
            }

            Camera = UnityEngine.Camera.main;
            _cinemachineCamera = Object.FindAnyObjectByType<CinemachineCamera>();
            if (_cinemachineCamera == null)
            {
                Debug.LogError("[CameraService] CinemachineCamera was not found in the active scene.");
                return;
            }

            CameraObject = _cinemachineCamera.gameObject;
            _thirdPersonFollow = _cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();

            if (_cameraTarget == null)
            {
                _cameraTarget = new GameObject("CameraTarget").transform;
            }

            _currentRotation = Vector2.zero;
            _cameraTarget.SetParent(_playerService.PlayerTransform, false);
            _cameraTarget.localPosition = new Vector3(0, 1.4f, 0);
            _cameraTarget.localRotation = Quaternion.identity;

            _cinemachineCamera.Target.TrackingTarget = _cameraTarget;
        }

        public void ChangeDistance(float newDistance)
        {
            if (_thirdPersonFollow != null)
            {
                _thirdPersonFollow.CameraDistance = newDistance;
            }
        }

        public void SetRotationAngle(Vector2 deltaRotation)
        {
            if (deltaRotation == Vector2.zero || _cameraTarget == null)
            {
                return;
            }

            SetCameraAngle(_currentRotation + deltaRotation);
        }

        public void SetCameraAngle(Vector2 rotation)
        {
            if (_cameraTarget == null)
            {
                return;
            }

            _currentRotation = rotation;
            _currentRotation.x %= 360f;
            _currentRotation.y = Mathf.Clamp(_currentRotation.y, -70f, 70f);

            _cameraTarget.localRotation = Quaternion.Euler(_currentRotation.y, 0, 0);
            CameraRotationChanged?.Invoke();
        }

        public Quaternion GetCameraRotation() =>
            _cameraTarget != null ? _cameraTarget.rotation : Quaternion.identity;

        public Vector2 GetCameraAngle() => _currentRotation;
    }
}
