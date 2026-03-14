using System;
using Infrastructure.Services.Player.Input;
using UnityEngine;

namespace Infrastructure.Services.Player.Animator
{
    public class PlayerAnimatorService : IPlayerAnimatorService
    {
        #region Public Events

        public event Action OnGrabGun;
        public event Action OnGrabGunEnded;
        public event Action OnShootEnded;
        public event Action OnAttackEnded;
        public event Action OnPhysicalAttack;
        
        #endregion
        
        #region Hashes
        
        private readonly int _isMoving = UnityEngine.Animator.StringToHash("isMoving");
        private readonly int _isFalling = UnityEngine.Animator.StringToHash("isFalling");
        
        private readonly int _jump = UnityEngine.Animator.StringToHash("Jump");
        private readonly int _land = UnityEngine.Animator.StringToHash("Land");
        private readonly int _physicalAttack = UnityEngine.Animator.StringToHash("PhysicalAttack");
        private readonly int _grabGun = UnityEngine.Animator.StringToHash("GrabGun");
        private readonly int _magicAttack = UnityEngine.Animator.StringToHash("MagicAttack");
        private readonly int _getHit = UnityEngine.Animator.StringToHash("TakeDamage");
        private readonly int _die = UnityEngine.Animator.StringToHash("Die");
        
        private readonly int _moveSpeed = UnityEngine.Animator.StringToHash("MoveSpeed");
        private readonly int _moveSpeedJump = UnityEngine.Animator.StringToHash("MoveSpeedJump");

        private readonly int _moveX = UnityEngine.Animator.StringToHash("moveX");
        private readonly int _moveY = UnityEngine.Animator.StringToHash("moveY");
        
        private readonly int _turn = UnityEngine.Animator.StringToHash("Turn");
        
        #endregion
        
        #region Private Fields
        
        private bool _isMovingCheck = false;
        private bool _isFallingCheck = false;

        private float _currentMoveSpeed = 0;
        private Vector2 _savedVector = Vector2.zero;
        
        private UnityEngine.Animator _animator;

        private  IFightInputService _fightInputService;
        
        private readonly IPlayerService _playerService;
        
        #endregion

        private PlayerAnimatorService(IPlayerService playerService)
        {
            _playerService = playerService;
        }
        
        public void InstallService()
        {
            _animator = _playerService.PlayerObject.GetComponent<UnityEngine.Animator>();
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

        public void SetTurnValue(float value)
        {
            _animator.SetFloat(_turn, value);
        }

        public void TriggerJump(bool isTrigger = true)
        {
            if (isTrigger) _animator.SetTrigger(_jump);
            else _animator.ResetTrigger(_jump);
        }

        public void TriggerLand() =>  _animator.SetTrigger(_land);

        public void TriggerPhysicalAttack() => _animator.SetTrigger(_physicalAttack);
        public void TriggerMagicAttack() => _animator.SetTrigger(_magicAttack);
        public void TriggerGrabGun() => _animator.SetTrigger(_grabGun);

        public void TriggerHit() => _animator.SetTrigger(_getHit);

        public void TriggerDeath()
        {
            _animator.SetTrigger(_die);
        }

        public void ChangeMoveSpeed(float newSpeed)
        {
            _animator.SetFloat(_moveSpeed, Mathf.Max(0, newSpeed-3) / 5);
            _currentMoveSpeed = newSpeed;
            ChangeMoveSpeedJump();
            SetMoveVector(_savedVector);
        }

        public void SetMoveVector(Vector2 vector)
        {
            _savedVector = vector;
            
            _animator.SetFloat(_moveX, vector.x);
            _animator.SetFloat(_moveY, vector.y);
        }

        private void ChangeMoveSpeedJump()
        {
            float jumpSpeed;
            if (_currentMoveSpeed <= 2 || !_isMovingCheck) jumpSpeed = 0;
            else if (_currentMoveSpeed >= 4) jumpSpeed = 1;
            else jumpSpeed = (_currentMoveSpeed - 2) / 2;
            _animator.SetFloat(_moveSpeedJump, jumpSpeed);
        }
        
        public void ProcessAnimationEvent(string eventId)
        {
            switch (eventId)
            {
                case "GrabGun":
                    OnGrabGun?.Invoke();
                    break;
                case "GrabGunEnded":
                    OnGrabGunEnded?.Invoke();
                    break;
                case "AttackEnded":
                    OnAttackEnded?.Invoke();
                    break;
                case "ShootEnded":
                    OnShootEnded?.Invoke();
                    break;
                case "PhysicalAttack":
                    OnPhysicalAttack?.Invoke();
                    break;
            }
        }
    }
}