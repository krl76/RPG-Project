using System;
using System.Collections.Generic;
using Data.Configs;
using Features.Combat;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Camera;
using Infrastructure.Services.Events;
using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    /// <summary>
    /// Обрабатывает физические и магические атаки игрока.
    /// </summary>
    public class PlayerCombat : MonoBehaviour, IPlayerMagicSubscriber
    {
        [Header("Physical Attack")]
        [SerializeField] private Transform _meleeAttackPoint; 
        [SerializeField] private LayerMask _enemyLayer;
        
        [Header("Magic Attack")]
        [SerializeField] private GameObject _magicProjectilePrefab;
        [SerializeField] private Transform _shootPoint;

        private IGameObjectFactory _gameObjectFactory;
        private IPlayerAnimatorService _animator;
        private IConfigDataProvider _configDataProvider;
        private ICombatAudioService _combatAudioService;
        private ICameraService _cameraService;

        [Inject]
        private void Construct(
            IGameObjectFactory gameObjectFactory,
            IConfigDataProvider configDataProvider,
            IPlayerAnimatorService playerAnimatorService,
            ICombatAudioService combatAudioService,
            ICameraService cameraService)
        {
            _gameObjectFactory = gameObjectFactory;
            _configDataProvider = configDataProvider;
            _animator = playerAnimatorService;
            _combatAudioService = combatAudioService;
            _cameraService = cameraService;
        }

        private void Start()
        {
            EventBus.Subscribe(this);
            _animator.OnPhysicalAttack += PhysicalAttack;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe(this);
            _animator.OnPhysicalAttack -= PhysicalAttack;
        }

        private void PhysicalAttack()
        {
            var config = _configDataProvider.GetPlayerStatsConfig();
            _combatAudioService.PlayPlayerMeleeAttack();
            
            Collider[] hitColliders = Physics.OverlapSphere(_meleeAttackPoint.position, config.MeleeHitRadius, _enemyLayer);
            HashSet<IDamageable> damagedVictims = new();

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var victim) && victim.IsAlive)
                {
                    if (damagedVictims.Add(victim))
                    {
                        victim.TakeDamage(config.PhysicalDamage);
                    }
                }
            }
        }

        public void OnMagicUsed(float remainingTime, float totalDuration)
        {
            var config = _configDataProvider.GetPlayerStatsConfig();
            _combatAudioService.PlayPlayerShot();
            Vector3 moveDirection = ResolveMagicAttackDirection();
            Quaternion projectileRotation = moveDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(moveDirection, Vector3.up)
                : (_shootPoint != null ? _shootPoint.rotation : transform.rotation);
            var projectile = _gameObjectFactory
                .Instantiate(_magicProjectilePrefab, _shootPoint.position, projectileRotation);

            if (projectile.TryGetComponent(out MagicProjectile magicProjectile))
            {
                magicProjectile.SetMoveDirection(moveDirection);
                magicProjectile.Setup(config.MagicDamage, config.ProjectileSpeed, 15);
            }
        }

        private Vector3 ResolveMagicAttackDirection()
        {
            Vector3 fallbackDirection = _shootPoint != null ? _shootPoint.forward : transform.forward;
            UnityEngine.Camera activeCamera = _cameraService?.Camera;
            if (activeCamera == null)
            {
                return fallbackDirection;
            }

            Ray aimRay = activeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimPoint = aimRay.origin + aimRay.direction * 100f;
            int ignoredLayerMask = 1 << transform.root.gameObject.layer;
            int raycastMask = ~ignoredLayerMask;
            if (Physics.Raycast(aimRay, out RaycastHit hitInfo, 1000f, raycastMask, QueryTriggerInteraction.Ignore))
            {
                aimPoint = hitInfo.point;
            }

            Vector3 origin = _shootPoint != null ? _shootPoint.position : transform.position;
            Vector3 moveDirection = aimPoint - origin;
            return moveDirection.sqrMagnitude > 0.0001f
                ? moveDirection.normalized
                : fallbackDirection;
        }

        public void OnMagicReady()
        {
            // заглушка из-за наследования подписчика
        }
    }
}
