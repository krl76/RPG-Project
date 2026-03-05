using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.Configs;
using Features.Player;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Player.Animator;
using Input.PlayerInput;
using UnityEngine;

namespace Infrastructure.Services.Player.Input
{
    public class FightInputService : IFightInputService
    {
        private bool _isAttackStarted = false;
        private bool _isMagicAttackAvailible = true;
        private bool _isAiming;
        private CancellationTokenSource _cancellationTokenSource;

        private PlayerMovement _movement;
        private PlayerStatsConfig _config;

        private readonly InputManager _inputManager;
        private readonly IPlayerService _playerService;
        private readonly IMovementInputService _movementInputService;
        private readonly ICameraService _cameraService;
        private readonly IPlayerAnimatorService _animatorService;
        private readonly IConfigDataProvider _configDataProvider;

        public FightInputService(InputManager inputManager,
            IPlayerService playerService,
            IMovementInputService movementInputService,
            ICameraService cameraService,
            IPlayerAnimatorService animatorService,
            IConfigDataProvider configDataProvider)
        {
            _playerService = playerService;
            _inputManager = inputManager;
            _movementInputService = movementInputService;
            _cameraService = cameraService;
            _animatorService = animatorService;
            _configDataProvider = configDataProvider;
        }
        
        public void InstallService()
        {
            _movement = _playerService.PlayerObject.GetComponent<PlayerMovement>();
            
            _config = _configDataProvider.GetPlayerStatsConfig();
            _animatorService.SetFightInputService(this);
            
            _inputManager.Actions.SwordAttack += OnPhysicalAttack;
            _inputManager.Actions.MagicAttack += OnGrabGun;
            _inputManager.Actions.SwordAttack += OnMagicAttack; 
            
            _animatorService.OnGrabGunEnded += AttackEnd;
            _animatorService.OnAttackEnded += AttackEnd;
        }

        public void UninstallService()
        {
            _inputManager.Actions.SwordAttack -= OnPhysicalAttack;
            _inputManager.Actions.MagicAttack -= OnGrabGun;
            _inputManager.Actions.SwordAttack -= OnMagicAttack;
            
            _animatorService.OnGrabGunEnded -= AttackEnd;
            _animatorService.OnAttackEnded -= AttackEnd;
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        public void AttackEnd()
        {
            _isAttackStarted = false;
            _isAiming = false;
            _movementInputService.CanMove = true;
            _movementInputService.ContinueMoveAfterAction();
        }
        
        private void OnPhysicalAttack()
        {
            if (_isAiming) return; 
            if (!AttackStart()) return;
            
            _animatorService.TriggerPhysicalAttack();
        }
        
        private void OnGrabGun()
        {
            if (_isAiming || !_isMagicAttackAvailible) return;
            if (!AttackStart()) return;
            
            _isAiming = true;
            _animatorService.TriggerGrabGun();
        }

        private void OnMagicAttack()
        {
            if (!_isAiming) return;
            
            _isAiming = false;
            _isMagicAttackAvailible = false;
            
            _cancellationTokenSource = new CancellationTokenSource();
            WaitMagicCooldown(_cancellationTokenSource).Forget();
            
            _animatorService.TriggerMagicAttack();

            // TODO: Вызвать логику Raycast/Spawn пули
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

        private async UniTask WaitMagicCooldown(CancellationTokenSource cancellationTokenSource)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_config.MagicAttackCooldown), 
                ignoreTimeScale: false, 
                PlayerLoopTiming.Update, 
                cancellationTokenSource.Token);
            _isMagicAttackAvailible = true;
        }
    }
}