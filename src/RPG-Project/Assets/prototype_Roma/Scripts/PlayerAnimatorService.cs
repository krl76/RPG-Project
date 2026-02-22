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
        
        private bool _isMovingCheck = false;
        private bool _isFallingCheck = false;

        private float _currentMoveSpeed = 0;
        
        private Animator _animator;

        private readonly IFightInputService _fightInputService;
        private readonly IPlayerService _playerService;

        private PlayerAnimatorService(IFightInputService fightInputService, IPlayerService playerService)
        {
            _fightInputService = fightInputService;
            _playerService = playerService;
        }
        
        public void InstallService()
        {
            _animator = _playerService.Player.GetComponent<Animator>();
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
            _playerService.Player.GetComponent<DeathEffectBehaviour>().PlayEffect();
        }

        public void ChangeMoveSpeed(float newSpeed)
        {
            _animator.SetFloat(_moveSpeed, newSpeed/10);
            _currentMoveSpeed = newSpeed;
            ChangeMoveSpeedJump();
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