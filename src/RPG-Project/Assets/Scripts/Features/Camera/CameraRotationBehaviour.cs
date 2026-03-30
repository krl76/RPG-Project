using Core.Gameplay.State;
using Infrastructure.Services.Camera;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Features.Camera
{
    /// <summary>
    /// Считывает ввод мыши и поворачивает игровую камеру.
    /// </summary>
    public class CameraRotationBehaviour : MonoBehaviour
    {
        [SerializeField] private float _sensitivity = 20f;

        private ICameraService _cameraService;
        private IGameStateService _gameStateService;

        [Inject]
        private void Construct(ICameraService cameraService, IGameStateService gameStateService)
        {
            _cameraService = cameraService;
            _gameStateService = gameStateService;
        }

        private void Start()
        {
            ApplyState(_gameStateService.CurrentState);
            _gameStateService.StateChanged += OnGameStateChanged;
        }

        private void LateUpdate()
        {
            if (_gameStateService.CurrentState != GameState.Gameplay || Mouse.current == null)
            {
                return;
            }

            Vector2 delta = Mouse.current.delta.ReadValue();
            if (delta.sqrMagnitude <= 0.005f)
            {
                return;
            }

            Vector2 rotationDelta = new Vector2(delta.x, -delta.y) * (_sensitivity * 0.05f);
            _cameraService.SetRotationAngle(rotationDelta);
        }

        private void OnDestroy()
        {
            if (_gameStateService != null)
            {
                _gameStateService.StateChanged -= OnGameStateChanged;
            }

            UnlockCursor();
        }

        private void OnGameStateChanged(GameState state)
        {
            ApplyState(state);
        }

        private static void ApplyState(GameState state)
        {
            if (state == GameState.Gameplay)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            UnlockCursor();
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
