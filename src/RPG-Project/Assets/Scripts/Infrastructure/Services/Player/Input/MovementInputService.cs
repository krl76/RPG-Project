using Features.Player;
using Infrastructure.Services.Player.Animator;
using Input.PlayerInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Services.Player.Input
{
    /// <summary>
    /// Сервис обработки движения, спринта и прыжка игрока.
    /// </summary>
    public class MovementInputService : IMovementInputService
    {
        public bool CanMove { get; set; }
        public bool IsMoving { get; set; }
        public Vector2 MoveVector { get; set; }
        
        private bool _isMoveAfterActionContinue = false;
        private bool _isSprintRequested = false;
        private bool _isSprintActive = false;
        private Vector2 _continueMoveVector;
        
        private readonly InputManager _inputManager;
        private readonly IPlayerService _playerService;
        private readonly IPlayerAnimatorService _playerAnimatorService;

        private PlayerMovement _playerMovement;

        public MovementInputService(InputManager inputManager, IPlayerService playerService,
            IPlayerAnimatorService playerAnimatorService)
        {
            _inputManager = inputManager;
            _playerService = playerService;
            _playerAnimatorService = playerAnimatorService;
        }
        
        public void InstallService()
        {
            CanMove = true;
            IsMoving = false;
            MoveVector = Vector2.zero;
            _continueMoveVector = Vector2.zero;
            _isSprintRequested = false;
            _isSprintActive = false;

            _playerMovement = _playerService.PlayerObject.GetComponent<PlayerMovement>();
            
            _inputManager.Actions.Move += OnMove;

            _inputManager.Actions.Jump += OnJump;

            _inputManager.Actions.Sprint += OnSprint;
        }

        public void UninstallService()
        {
            _inputManager.Actions.Move -= OnMove;

            _inputManager.Actions.Jump -= OnJump;
            
            _inputManager.Actions.Sprint -= OnSprint;
        }

        public void ContinueMoveAfterAction()
        { 
            if (!_isMoveAfterActionContinue) return;
            IsMoving = true;
            MoveVector = _continueMoveVector;
            _playerMovement.OnMovementChange();
            ApplySprintState();
        }

        private void OnJump()
        {
            _playerMovement.OnJumpPressed();
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            if (context.started)
            { _isMoveAfterActionContinue = true;
                IsMoving = true; }
            if (context.canceled)
            { _isMoveAfterActionContinue = false;
                IsMoving = false; }
            
            _continueMoveVector = context.ReadValue<Vector2>();
            if (!CanMove) return;
            
            if (IsMoving) MoveVector = _continueMoveVector;
            else MoveVector = Vector2.zero;
            _playerMovement.OnMovementChange();
            ApplySprintState();
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            _isSprintRequested = !context.canceled;
            ApplySprintState();
        }

        private void ApplySprintState()
        {
            bool shouldSprint = _isSprintRequested
                && CanMove
                && IsMoving
                && MoveVector.y >= 0f;

            if (_isSprintActive == shouldSprint)
            {
                return;
            }

            _isSprintActive = shouldSprint;
            _playerMovement.SprintChange(_isSprintActive);
        }
    }
}
