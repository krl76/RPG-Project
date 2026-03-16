using System;
using Data.Configs;
using Features.Combat;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Enemy;
using Infrastructure.Services.Player;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Features.Enemy
{[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(EnemyAnimation))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        public bool IsAlive => _currentHealth > 0;
        [field: SerializeField] public EnemyConfig Config;

        [Header("References")]
        [SerializeField] private Transform _meleeAttackPoint;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private LayerMask _playerLayer;

        private float _currentHealth;
        private float _lastAttackTime;
        private Transform _playerTransform;
        
        private NavMeshAgent _agent;
        private EnemyAnimation _animator;
        private IHealthFeedback _healthFeedback;
        private IGameObjectFactory _gameObjectFactory;
        private IEnemyService _enemyService;
        private IPlayerService _playerService;

        [Inject]
        private void Construct(
            IPlayerService playerService,
            IGameObjectFactory gameObjectFactory,
            IEnemyService enemyService)
        {
            _playerService = playerService;
            _gameObjectFactory = gameObjectFactory;
            _enemyService = enemyService;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<EnemyAnimation>();
            _healthFeedback = GetComponentInChildren<IHealthFeedback>();
            
            _currentHealth = Config.MaxHealth;

            _enemyService.Register(this);
        }

        private void Start()
        {
            if (_playerService == null) ProjectContext.Instance.Container.Resolve<IPlayerService>();
            
            _playerTransform = _playerService.PlayerTransform;
        }

        private void Update()
        {
            if (!IsAlive) return;
            if (!_playerTransform)
            {
                if (!_playerService.PlayerTransform) return;
                _playerTransform = _playerService.PlayerTransform;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            
            if (distanceToPlayer <= Config.ChaseRange)
            {
                if (distanceToPlayer <= Config.AttackRange)
                {
                    _agent.isStopped = true;
                    
                    _animator.SetIsRunning(false);
                    LookAtPlayer();
            
                    if (Time.time >= _lastAttackTime + Config.AttackCooldown)
                    {
                        Attack();
                    }
                }
                else if (distanceToPlayer >= Config.AttackRange + Config.AttackRange * 0.02)
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(_playerTransform.position);
                    _animator.SetIsRunning(true);
                }
            }
            else
            {
                _agent.isStopped = true;
                _animator.SetIsRunning(false);
            }
        }

        private void LookAtPlayer()
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                3f * Time.deltaTime);
        }

        private void Attack()
        {
            _lastAttackTime = Time.time;
            if (Config.Type == EnemyType.Melee)
                _animator.PlayAttack();
            else
                _animator.PlayMagicAttack();
        }

        public void OnPhysicalAttack() // ANIMATION EVENT
        {
            Collider[] hitColliders = Physics.OverlapSphere(_meleeAttackPoint.position,
                Config.HitRadius, _playerLayer);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var victim) && victim.IsAlive)
                {
                    victim.TakeDamage(Config.Damage);
                    
                    break; 
                }
            }
        }

        private void OnMagicAttack() // ANIMATION EVENT
        {
            var projectile = _gameObjectFactory
                .Instantiate(Config.ProjectilePrefab, _shootPoint.position, transform.rotation);
            projectile.GetComponent<MagicProjectile>().Setup(Config.Damage, Config.ProjectileSpeed);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;

            _currentHealth -= amount;
            
            _healthFeedback.OnHealthChanged(_currentHealth, Config.MaxHealth);

            if (_currentHealth <= 0)
            {
                _agent.isStopped = true;
                _animator.PlayDeath();
                GetComponent<Collider>().enabled = false;
                Destroy(gameObject, 3f);
            }
            else
            {
                _animator.PlayHit();
            }
        }
    }
}