using Infrastructure.Services.Camera;
using Infrastructure.Services.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Features.Camera
{
    public class CameraRotationBehaviour : MonoBehaviour, IGameStateSubscriber
    {
        [SerializeField] private float _sensitivity = 20f;
        
        private ICameraService _cameraService;

        [Inject]
        private void Construct(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        private void Start()
        {
            EventBus.Subscribe(this);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (Mouse.current == null) return;

            Vector2 delta = Mouse.current.delta.ReadValue();
            
            if (delta.sqrMagnitude > 0.005f)
            {
                Vector2 rotationDelta = new Vector2(delta.x, -delta.y) * (_sensitivity * 0.05f);
                
                _cameraService.SetRotationAngle(rotationDelta);
            }
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(this);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnGameOver()
        {
            gameObject.SetActive(false);
        }

        public void OnGameRestarted()
        {
            //
        }
    }
}