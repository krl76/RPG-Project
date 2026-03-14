using System;
using Data.Configs;
using Features.Combat;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Events;
using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace Features.Player
{
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

        [Inject]
        private void Construct(
            IGameObjectFactory gameObjectFactory,
            IConfigDataProvider configDataProvider,
            IPlayerAnimatorService playerAnimatorService)
        {
            _gameObjectFactory = gameObjectFactory;
            _configDataProvider = configDataProvider;
            _animator = playerAnimatorService;
        }

        private void Start()
        {
            _animator.OnPhysicalAttack += PhysicalAttack;
        }

        private void OnDestroy()
        {
            _animator.OnPhysicalAttack -= PhysicalAttack;
        }

        private void PhysicalAttack()
        {
            var config = _configDataProvider.GetPlayerStatsConfig();
            
            Collider[] hitColliders = Physics.OverlapSphere(_meleeAttackPoint.position, config.MeleeHitRadius, _enemyLayer);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var victim) && victim.IsAlive)
                {
                    victim.TakeDamage(config.PhysicalDamage);
                    
                    break; 
                }
            }
        }

        public void OnMagicUsed(float cooldownDuration)
        {
            var config = _configDataProvider.GetPlayerStatsConfig();
            
            var projectile = _gameObjectFactory
                .Instantiate(_magicProjectilePrefab, _shootPoint.position, transform.rotation);
            projectile.GetComponent<MagicProjectile>().Setup(config.MagicDamage, config.ProjectileSpeed);
        }

        public void OnMagicReady()
        {
            // заглушка из-за наследования подписчика
        }
    }
}