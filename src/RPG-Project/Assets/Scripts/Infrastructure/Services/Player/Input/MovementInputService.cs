using Features.Player;
using Infrastructure.Services.Player.Animator;
using Input.PlayerInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Services.Player.Input
{
    public class MovementInputService : IMovementInputService
    {
        public bool CanMove { get; set; }
        public bool IsMoving { get; set; }
        public Vector2 MoveVector { get; set; }
        
        private bool _isMoveAfterActionContinue = false;
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
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            _playerMovement.SprintChange(!context.canceled);
        }
    }
}