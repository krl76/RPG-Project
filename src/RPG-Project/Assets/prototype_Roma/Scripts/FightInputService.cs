using UnityEngine;

namespace prototype_Roma.Scripts
{
    public class FightInputService : IFightInputService
    {
        private bool _isAttackStarted = false;
        
        private PlayerMovementBehaviour _movement;
        
        private readonly InputManager _inputManager;
        private readonly IPlayerService _playerService;
        private readonly IMovementInputService _movementInputService;
        private readonly ICameraService _cameraService;
        private readonly IPlayerAnimatorService _animatorService;

        public FightInputService(InputManager inputManager,
            IPlayerService playerService,
            IMovementInputService movementInputService,
            ICameraService cameraService,
            IPlayerAnimatorService animatorService)
        {
            _playerService = playerService;
            _inputManager = inputManager;
            _movementInputService = movementInputService;
            _cameraService = cameraService;
            _animatorService = animatorService;
        }
        
        public void InstallService()
        {
            _playerMovement = _playerService.PlayerObject.GetComponent<PlayerMovementBehaviour>();
            
            _inputManager.Actions.AttackEnemy += OnPhysicalAttack;
            _inputManager.Actions.UseShield += OnMagicAttack; // другой инпут
        }

        public void UninstallService()
        {
            _inputManager.Actions.AttackEnemy -= OnPhysicalAttack;
            _inputManager.Actions.UseShield -= OnMagicAttack; // другой инпут
        }

        public void AttackEnd()
        {
            _isAttackStarted = false;
            _movementInputService.CanMove = true;
            _movementInputService.ContinueMoveAfterAction();
        }
        
        private void OnPhysicalAttack()
        {
            if (!AttackStart()) return;
            _animatorService.TriggerAttack();
        }

        private void OnMagicAttack()
        {
            if (!AttackStart()) return;
            _animatorService.TriggerAttack();
        }

        private bool AttackStart()
        {
            if (_isAttackStarted || _movement.IsFalling) return false;
            _isAttackStarted = true;
            _movementInputService.MoveVector = Vector2.zero;
            _movementInputService.IsMoving = false;
            _movementInputService.CanMove = false;
            return true;
        }
    }
}