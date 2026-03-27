using System;
using Data.Configs;
using Features.Combat;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Audio;
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
        private ICombatAudioService _combatAudioService;

        [Inject]
        private void Construct(
            IGameObjectFactory gameObjectFactory,
            IConfigDataProvider configDataProvider,
            IPlayerAnimatorService playerAnimatorService,
            ICombatAudioService combatAudioService)
        {
            _gameObjectFactory = gameObjectFactory;
            _configDataProvider = configDataProvider;
            _animator = playerAnimatorService;
            _combatAudioService = combatAudioService;
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

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var victim) && victim.IsAlive)
                {
                    victim.TakeDamage(config.PhysicalDamage);
                    
                    break; 
                }
            }
        }

        public void OnMagicUsed(float remainingTime, float totalDuration)
        {
            var config = _configDataProvider.GetPlayerStatsConfig();
            _combatAudioService.PlayPlayerShot();
            var projectile = _gameObjectFactory
                .Instantiate(_magicProjectilePrefab, _shootPoint.position, transform.rotation);
            projectile.GetComponent<MagicProjectile>().Setup(config.MagicDamage, config.ProjectileSpeed, 15);
        }

        public void OnMagicReady()
        {
            // заглушка из-за наследования подписчика
        }
    }
}
