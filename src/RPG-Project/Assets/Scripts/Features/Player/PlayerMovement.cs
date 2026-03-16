using System;
using System.Collections;
using Data.Configs;
using Features.Combat;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Animator;
using Infrastructure.Services.Player.Input;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public bool IsFalling = false;
        
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _distanceToGround;
        [SerializeField] private Transform _RaycastOrigin;
        [SerializeField] private Vector3 _boxCastHalfSize;
        
        [SerializeField] private float _turnSensitivity = 0.2f;
        [SerializeField] private float _turnReturnSpeed = 10f; 
        
        private float _currentTurnValue = 0f;
        private float _coyoteTime = 0.08f;
        private float _jumpBufferTime = 0.1f;

        private float _currentMoveSpeed = 4;
        private float _changedMoveSpeed = 4;
        private float _speedChangeCoefficent = 2;
        private float _sprintSpeedCoefficent = 1f;

        private Vector3 _currentMovement;
        private bool _isJumpStarted = false;
        private bool _isLanded = true;
        private Quaternion _moveVectorRotation = Quaternion.identity;
        private float _rotationSlow = 1;
        private float _hitSlow = 1;
        private Vector2 _rotatedMovement;

        private Vector2 _jumpStartImpulse;

        private float _jumpBufferTimer = 0;
        private float _coyoteTimer = 0;
        
        private float _resetJumpTimer = 0;
        private bool _isJumpHappend = false;

        private bool _isOnGround = false;
        
        private const float _gravity = -9.8f;
        
        private float _animationSmoothTime = 0.2f;
        private Vector2 _currentAnimVector;
        private Vector2 _animVelocity;
        
        private IMovementInputService _movementInput;
        private IPlayerService _playerService;
        private PlayerStatsConfig _config;
        private ICameraService _cameraService;
        private IPlayerAnimatorService _animator;

        private IConfigDataProvider _configDataProvider;
        private IDamageable _playerDamageable;
    
        [Inject]
        private void Construct(IMovementInputService movementInputService, IPlayerService playerService,
            IConfigDataProvider configDataProvider, ICameraService cameraService, 
            IPlayerAnimatorService playerAnimatorService)
        {
            _movementInput = movementInputService;
            _playerService = playerService;
            _cameraService = cameraService;
            _configDataProvider = configDataProvider;
            _animator = playerAnimatorService;
        }

        private void Start() // delete when proper bootstrap setup
        {
            if (_playerService == null) ProjectContext.Instance.Container.Resolve<IPlayerService>();
            if (_cameraService == null) ProjectContext.Instance.Container.Resolve<ICameraService>();
            if (_configDataProvider == null) ProjectContext.Instance.Container.Resolve<IConfigDataProvider>();
            if (_animator == null) ProjectContext.Instance.Container.Resolve<IPlayerAnimatorService>();

            _config = _configDataProvider?.GetPlayerStatsConfig();
        }

        public void OnJumpPressed()
        {
            if (_isOnGround | _coyoteTimer > 0)
            {
                _animator.TriggerJump();
                _resetJumpTimer = 0.2f;
                _isJumpHappend = false;
                return;
            }
            _jumpBufferTimer = _jumpBufferTime;
        } 
        
        public void JumpHappend()
        {
            _isJumpHappend = true;
            _isJumpStarted = true;
            _jumpStartImpulse = _movementInput.MoveVector;
            _currentMovement.y = _config.JumpVelocity;
        }

        public void OnMovementChange()
        {
            Vector2 move;
            if (IsFalling)
            {
                move = _jumpStartImpulse + _movementInput.MoveVector;
                move.Normalize();
            }
            else move = _movementInput.MoveVector;
            _rotatedMovement = _moveVectorRotation * move;
            
        }

        public void SprintChange(bool isAcceleration)
        {
            if (isAcceleration) _changedMoveSpeed = _config.RunSpeed;
            else _changedMoveSpeed = _config.WalkSpeed;
            StartCoroutine(ChangeSpeed());
        }
        
        private void OnEnable()
        {
            _cameraService.CameraRotationChanged += ChangePlayerRotation;

            _playerDamageable = GetComponent<PlayerHealth>();
        }

        private void OnDisable()
        {
            _cameraService.CameraRotationChanged -= ChangePlayerRotation;
        }


        private void Update()
        {
            if (_animator == null) return;
            
            UpdateTimers();
            
            if (_movementInput.IsMoving) _currentMovement = new Vector3(_rotatedMovement.x, _currentMovement.y, _rotatedMovement.y);
            else _currentMovement = new Vector3(0, _currentMovement.y, 0);
            
            _animator.SetMoveBool(_movementInput.IsMoving);
            
            Vector2 inputMove = _movementInput.IsMoving ? _movementInput.MoveVector : Vector2.zero;

            if (inputMove.sqrMagnitude > 0.01f) 
            {
                inputMove = inputMove.normalized; 
            }

            float safeCurrentSpeed = Mathf.Clamp(_currentMoveSpeed, _config.WalkSpeed, _config.RunSpeed);

            float sprintProgress = Mathf.InverseLerp(_config.WalkSpeed, _config.RunSpeed, safeCurrentSpeed);

            float blendTreeMultiplier = Mathf.Lerp(0.5f, 1f, sprintProgress);

            Vector2 targetMove = inputMove * blendTreeMultiplier;

            _currentAnimVector = Vector2.SmoothDamp(
                _currentAnimVector, 
                targetMove, 
                ref _animVelocity, 
                _animationSmoothTime
            );

            _animator.SetMoveVector(_currentAnimVector);
            
            UpdateGravity();
            
            _characterController.Move(Vector3.Scale(_currentMovement,
                new Vector3(_rotationSlow, 1, _rotationSlow)) * (_config.MoveSpeedCoef * _sprintSpeedCoefficent * _hitSlow * Time.deltaTime));    
            
            _currentTurnValue = Mathf.Lerp(_currentTurnValue, 0f, Time.deltaTime * _turnReturnSpeed);
            
            _animator.SetTurnValue(_currentTurnValue);
        }

        private void UpdateTimers()
        {
            if (_jumpBufferTimer > 0) _jumpBufferTimer -= Time.deltaTime;
            if (_coyoteTimer > 0) _coyoteTimer -= Time.deltaTime;
            if (_resetJumpTimer > 0) _resetJumpTimer -= Time.deltaTime;
            else if (!_isJumpHappend) _animator.TriggerJump(false);
        }

        private void UpdateGravity()
        {
            if (_isOnGround && !_isJumpStarted)
            {
                
                if (!_isLanded)
                {
                    _isLanded = true;
                    _animator.TriggerLand();
                    if (_jumpBufferTimer > 0)
                        OnJumpPressed();
                }
                IsFalling = false;
                OnMovementChange();
                _animator.SetFallBool(false);
                _currentMovement.y = -1;
                return;
            }
            _isLanded = false;
            IsFalling = true;
            _animator.SetFallBool(true);
            if (_currentMovement.y < 0.0f && _isJumpStarted)
            {
                _isJumpStarted = false;
            }
            if (_isJumpStarted) _currentMovement.y += (_gravity * _config.UpwardsMultiplier * Time.deltaTime);
            else _currentMovement.y += (_gravity * _config.DownwardsMultiplier * Time.deltaTime);
            _currentMovement.y = MathF.Min(_currentMovement.y, _config.VelocityLimit);
        }

        private void FixedUpdate()
        {
            bool state = Physics.BoxCast(_RaycastOrigin.position, _boxCastHalfSize,
                Vector3.down, transform.rotation, _distanceToGround, ~_playerLayer, QueryTriggerInteraction.Ignore);
            if (!state && _isOnGround && !_isJumpStarted) _coyoteTimer = _coyoteTime;
            _isOnGround = state;
        }

        private IEnumerator ChangeSpeed()
        {
            while (Mathf.Abs(_currentMoveSpeed - _changedMoveSpeed) > 0.05f)
            {
                _currentMoveSpeed = Mathf.Lerp(_currentMoveSpeed, _changedMoveSpeed,
                    _speedChangeCoefficent * Time.deltaTime);
                
                _sprintSpeedCoefficent = _currentMoveSpeed / _config.WalkSpeed;
                _animator.ChangeMoveSpeed(_currentMoveSpeed);
                
                yield return null; 
            } 

            _currentMoveSpeed = _changedMoveSpeed;
            _sprintSpeedCoefficent = _currentMoveSpeed / _config.WalkSpeed;
            _animator.ChangeMoveSpeed(_currentMoveSpeed);
        }

        private void Hit()
        {
            _hitSlow = 0.1f;
            StartCoroutine(ReturnNormalSpeed());
        }

        private IEnumerator ReturnNormalSpeed()
        {
            yield return new WaitForSeconds(0.5f);
            _hitSlow = 1;
        }
        private void ChangePlayerRotation()
        {
            float targetYaw = _cameraService.GetCameraAngle().x;
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw);
            
            _currentTurnValue = Mathf.Clamp(angleDifference * _turnSensitivity, -1f, 1f);
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);

            _moveVectorRotation = Quaternion.AngleAxis(-targetYaw, Vector3.forward);
            
            OnMovementChange();
        }
    }
}
