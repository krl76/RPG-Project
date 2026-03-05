using Infrastructure.Services.Camera;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Features.Camera
{
    public class CameraRotationBehaviour : MonoBehaviour
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

        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}