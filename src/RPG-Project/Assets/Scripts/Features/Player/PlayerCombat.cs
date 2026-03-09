using Data.Configs;
using Features.Combat;
using Infrastructure.Factories.Objects;
using Infrastructure.Providers.Configs;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Physical Attack")]
        [SerializeField] private Transform _meleeAttackPoint; 
        [SerializeField] private LayerMask _enemyLayer;
        
        [Header("Magic Attack")]
        [SerializeField] private GameObject _magicProjectilePrefab;
        [SerializeField] private Transform _shootPoint;

        private IGameObjectFactory _gameObjectFactory;
        private PlayerStatsConfig _config;

        [Inject]
        private void Construct(IGameObjectFactory gameObjectFactory, IConfigDataProvider configDataProvider)
        {
            _gameObjectFactory = gameObjectFactory;
            _config = configDataProvider.GetPlayerStatsConfig();
        }

        public void ExecutePhysicalHit()
        {
            if (_meleeAttackPoint == null) return;

            Collider[] hitColliders = Physics.OverlapSphere(_meleeAttackPoint.position, _config.MeleeHitRadius, _enemyLayer);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var victim) && victim.IsAlive)
                {
                    victim.TakeDamage(_config.PhysicalDamage);
                    
                    break; 
                }
            }
        }

        public void ExecuteMagicShoot()
        {
            if (_magicProjectilePrefab != null && _shootPoint != null)
            {
                _gameObjectFactory.Instantiate(_magicProjectilePrefab, _shootPoint.position, transform.rotation);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_meleeAttackPoint == null || _config == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_meleeAttackPoint.position, _config.MeleeHitRadius);
        }
    }
}