using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data.Configs;
using Features.Player;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Events;
using Infrastructure.Services.Player.Animator;
using Input.PlayerInput;
using UnityEngine;

namespace Infrastructure.Services.Player.Input
{
    /// <summary>
    /// Сервис обработки атак, прицеливания и отката магии игрока.
    /// </summary>
    public class FightInputService : IFightInputService
    {
        public float MagicCooldownRemaining =>
            _isMagicAttackAvailable ? 0f : Mathf.Max(0f, _magicCooldownEndTime - Time.time);

        public float MagicCooldownDuration => _magicCooldownDuration;

        private bool _isAttackStarted = false;
        private bool _isMagicAttackAvailable = true;
        private bool _isAiming;
        private float _magicCooldownEndTime;
        private float _magicCooldownDuration;
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
            IConfigDataProvider configDataProvider
            )
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
            ResetCombatState();

            _movement = _playerService.PlayerObject.GetComponent<PlayerMovement>();
            _config = _configDataProvider.GetPlayerStatsConfig();
            
            _animatorService.SetFightInputService(this);
            
            _inputManager.Actions.SwordAttack += OnPhysicalAttack;
            _inputManager.Actions.MagicAttack += OnGrabGun;
            _inputManager.Actions.SwordAttack += OnMagicAttack; 

            _animatorService.OnShootEnded += AttackEnd;
            _animatorService.OnAttackEnded += AttackEnd;
        }

        public void UninstallService()
        {
            _inputManager.Actions.SwordAttack -= OnPhysicalAttack;
            _inputManager.Actions.MagicAttack -= OnGrabGun;
            _inputManager.Actions.SwordAttack -= OnMagicAttack;
            
            _animatorService.OnShootEnded -= AttackEnd;
            _animatorService.OnAttackEnded -= AttackEnd;
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            ResetCombatState();
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
            if (_isAiming || !_isMagicAttackAvailable) return;
            if (!AttackStart()) return;
            
            _isAiming = true;
            _animatorService.TriggerGrabGun();
        }

        private void OnMagicAttack()
        {
            if (!_isAiming) return;
            
            _isAiming = false;
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            WaitMagicCooldown(
                _config.MagicAttackCooldown,
                _config.MagicAttackCooldown,
                _cancellationTokenSource).Forget();
            
            _animatorService.TriggerMagicAttack();
        }

        private bool AttackStart()
        {
            if (_isAttackStarted || _movement.IsFalling || _animatorService.IsHitStateActive) return false;
            
            _isAttackStarted = true;
            _movementInputService.MoveVector = Vector2.zero;
            _movementInputService.IsMoving = false;
            _movementInputService.CanMove = false;
            
            return true;
        }

        public void RestoreMagicCooldown(float remainingTime, float totalDuration)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (remainingTime <= 0f || totalDuration <= 0f)
            {
                _isMagicAttackAvailable = true;
                _magicCooldownDuration = 0f;
                _magicCooldownEndTime = 0f;
                EventBus.RaiseEvent<IPlayerMagicSubscriber>(sub => sub.OnMagicReady());
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            WaitMagicCooldown(remainingTime, totalDuration, _cancellationTokenSource).Forget();
        }

        private async UniTask WaitMagicCooldown(float remainingTime, float totalDuration, CancellationTokenSource cts)
        {
            _isMagicAttackAvailable = false;
            _magicCooldownDuration = totalDuration;
            _magicCooldownEndTime = Time.time + remainingTime;

            EventBus.RaiseEvent<IPlayerMagicSubscriber>(sub => sub.OnMagicUsed(remainingTime, totalDuration));

            bool isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(remainingTime), cancellationToken: cts.Token)
                .SuppressCancellationThrow();
            if (isCancelled)
            {
                return;
            }

            _isMagicAttackAvailable = true;
            _magicCooldownEndTime = 0f;
            EventBus.RaiseEvent<IPlayerMagicSubscriber>(sub => sub.OnMagicReady());
        }

        private void ResetCombatState()
        {
            _isAttackStarted = false;
            _isAiming = false;
            _isMagicAttackAvailable = true;
            _magicCooldownEndTime = 0f;
            _magicCooldownDuration = 0f;

            _movementInputService.CanMove = true;
        }
    }
}
