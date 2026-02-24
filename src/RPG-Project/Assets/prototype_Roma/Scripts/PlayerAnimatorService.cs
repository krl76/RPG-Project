using UnityEngine;

namespace prototype_Roma.Scripts
{
    public class PlayerAnimatorService : IPlayerAnimatorService
    {
        private readonly int _isMoving = Animator.StringToHash("isMoving");
        private readonly int _isFalling = Animator.StringToHash("isFalling");
        
        private readonly int _jump = Animator.StringToHash("Jump");
        private readonly int _land = Animator.StringToHash("Land");
        private readonly int _physicalAttack = Animator.StringToHash("PhysicalAttack");
        private readonly int _magicAttack = Animator.StringToHash("MagicAttack");
        private readonly int _getHit = Animator.StringToHash("TakeDamage");
        private readonly int _die = Animator.StringToHash("Die");
        
        private readonly int _moveSpeed = Animator.StringToHash("MoveSpeed");
        private readonly int _moveSpeedJump = Animator.StringToHash("MoveSpeedJump");

        private readonly int _moveX = Animator.StringToHash("moveX");
        private readonly int _moveY = Animator.StringToHash("moveY");
        
        private bool _isMovingCheck = false;
        private bool _isFallingCheck = false;

        private float _currentMoveSpeed = 0;
        private Vector2 _savedVector = Vector2.zero;
        
        private Animator _animator;

        private  IFightInputService _fightInputService;
        
        private readonly IPlayerService _playerService;

        private PlayerAnimatorService(IPlayerService playerService)
        {
            _playerService = playerService;
        }
        
        public void InstallService()
        {
            _animator = _playerService.PlayerObject.GetComponent<Animator>();
        }

        public void SetFightInputService(IFightInputService fightInputService)
        {
            _fightInputService = fightInputService;
        }

        public void ResetTriggersByHit()
        {
            _animator.ResetTrigger(_physicalAttack);
            _animator.ResetTrigger(_magicAttack);
            _fightInputService.AttackEnd();
        }

        public void SetMoveBool(bool state)
        {
            if (state == _isMovingCheck) return;
            _isMovingCheck = state;
            ChangeMoveSpeedJump();
            _animator.SetBool(_isMoving, state);
        }

        public void SetFallBool(bool state)
        {
            if (state == _isFallingCheck) return;
            _isFallingCheck = state;
            _animator.SetBool(_isFalling, state);
        }

        public void TriggerJump(bool isTrigger = true)
        {
            if (isTrigger) _animator.SetTrigger(_jump);
            else _animator.ResetTrigger(_jump);
        }

        public void TriggerLand() =>  _animator.SetTrigger(_land);

        public void TriggerPhysicalAttack() => _animator.SetTrigger(_physicalAttack);
        public void TriggerMagicAttack() => _animator.SetTrigger(_magicAttack);

        public void TriggerHit() => _animator.SetTrigger(_getHit);

        public void TriggerDeath()
        {
            _animator.SetTrigger(_die);
        }

        public void ChangeMoveSpeed(float newSpeed)
        {
            _animator.SetFloat(_moveSpeed, newSpeed/10);
            _currentMoveSpeed = newSpeed;
            ChangeMoveSpeedJump();
            SetMoveVector(_savedVector);
        }

        public void SetMoveVector(Vector2 vector)
        {
            float runCoef = 0.5f;
            _savedVector = vector;
            if (_currentMoveSpeed > 5) runCoef = 1;
            _animator.SetFloat(_moveX, vector.x * runCoef);
            _animator.SetFloat(_moveY, vector.y * runCoef);
        }

        private void ChangeMoveSpeedJump()
        {
            float jumpSpeed;
            if (_currentMoveSpeed <= 2 || !_isMovingCheck) jumpSpeed = 0; // меняем значения
            else if (_currentMoveSpeed >= 4) jumpSpeed = 1;
            else jumpSpeed = (_currentMoveSpeed - 2) / 2;
            _animator.SetFloat(_moveSpeedJump, jumpSpeed);
        }
    }
}