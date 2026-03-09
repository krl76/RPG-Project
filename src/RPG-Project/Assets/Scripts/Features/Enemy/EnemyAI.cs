using Data.Configs;
using Features.Combat;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Player;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Features.Enemy
{[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        public bool IsAlive => _currentHealth > 0;
        public EnemyConfig Config { get; private set; }

        [Header("References")]
        [SerializeField] private Transform _meleeAttackPoint;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private LayerMask _playerLayer;

        private float _currentHealth;
        private float _lastAttackTime;
        private Transform _playerTransform;
        
        private NavMeshAgent _agent;
        private Animator _animator;
        private IHealthFeedback _healthFeedback;
        private IGameObjectFactory _gameObjectFactory;

        [Inject]
        private void Construct(IPlayerService playerService, IGameObjectFactory gameObjectFactory)
        {
            _playerTransform = playerService.PlayerTransform;
            _gameObjectFactory = gameObjectFactory;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _healthFeedback = GetComponent<IHealthFeedback>();
            
            _currentHealth = Config.MaxHealth;
        }

        private void Update()
        {
            if (!IsAlive || _playerTransform == null || Config == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= Config.ChaseRange)
            {
                if (distanceToPlayer <= Config.AttackRange)
                {
                    _agent.isStopped = true;
                    _animator.SetBool("isMoving", false);
                    LookAtPlayer();

                    if (Time.time >= _lastAttackTime + Config.AttackCooldown)
                    {
                        Attack();
                    }
                }
                else
                {
                    _agent.isStopped = false;
                    _agent.SetDestination(_playerTransform.position);
                    _animator.SetBool("isMoving", true);
                }
            }
            else
            {
                _agent.isStopped = true;
                _animator.SetBool("isMoving", false);
            }
        }

        private void LookAtPlayer()
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                5f * Time.deltaTime);
        }

        private void Attack()
        {
            _lastAttackTime = Time.time;
            _animator.SetTrigger("Attack");
        }

        public void OnAttackFrame()
        {
            if (Config.Type == EnemyType.Melee)
            {
                if (_meleeAttackPoint == null) return;

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
            else if (Config.Type == EnemyType.Ranged && Config.ProjectilePrefab != null)
            {
                _gameObjectFactory.Instantiate(Config.ProjectilePrefab, _shootPoint.position, transform.rotation); 
            }
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;

            _currentHealth -= amount;
            
            _healthFeedback.OnHealthChanged(_currentHealth, Config.MaxHealth);
            
            _animator.SetTrigger("Hit");

            if (_currentHealth <= 0)
            {
                _agent.isStopped = true;
                _animator.SetTrigger("Die");
                GetComponent<Collider>().enabled = false;
                Destroy(gameObject, 3f);
            }
        }
    }
}