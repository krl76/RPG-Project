using Core.Gameplay.Save;
using Core.Gameplay.Save.Data;
using Data.Configs;
using Features.Combat;
using Features.Enemy.States;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Enemy;
using Infrastructure.Services.Player;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Features.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(EnemyAnimation))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        public bool IsAlive => _currentHealth > 0f;
        public float CurrentHealth => _currentHealth;
        public string SaveId => SceneObjectSaveId.Build(transform);
        public EnemyStateId CurrentStateId => _stateMachine?.CurrentStateId ?? EnemyStateId.None;
        [field: SerializeField] public EnemyConfig Config { get; private set; }

        [Header("References")]
        [SerializeField] private Transform _meleeAttackPoint;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private LayerMask _playerLayer;

        private float _currentHealth;
        private float _nextPrimaryAttackTime;
        private float _nextStrongAttackTime;
        private float _nextAirAttackTime;
        private float _nextFleeRepathTime;
        private float _actionTimeoutAt;
        private float _baseAgentSpeed;
        private float _bossAttackUnlockAt;

        private bool _isProvoked;
        private bool _isEnraged;
        private bool _actionImpactConsumed;

        private Transform _playerTransform;
        private IDamageable _playerDamageable;

        private EnemyActionType _currentAction = EnemyActionType.None;
        private GameObject _activeSustainedAttackEffect;
        private EnemyAnimation _enemyAnimation;
        private EnemyStateMachine _stateMachine;
        private NavMeshAgent _agent;
        private IHealthFeedback _healthFeedback;
        private IGameObjectFactory _gameObjectFactory;
        private IEnemyService _enemyService;
        private IEnemyModeService _enemyModeService;
        private IPlayerService _playerService;
        private ICombatAudioService _combatAudioService;

        [Inject]
        private void Construct(
            IPlayerService playerService,
            IGameObjectFactory gameObjectFactory,
            IEnemyService enemyService,
            IEnemyModeService enemyModeService,
            ICombatAudioService combatAudioService)
        {
            _playerService = playerService;
            _gameObjectFactory = gameObjectFactory;
            _enemyService = enemyService;
            _enemyModeService = enemyModeService;
            _combatAudioService = combatAudioService;
        }

        public bool IsActionInProgress => _currentAction != EnemyActionType.None;

        private void Awake()
        {
            if (Config == null)
            {
                Debug.LogError($"[EnemyAI] EnemyConfig is missing on {name}.", this);
                enabled = false;
                return;
            }

            _agent = GetComponent<NavMeshAgent>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _healthFeedback = GetComponentInChildren<IHealthFeedback>();

            _baseAgentSpeed = _agent.speed;
            _currentHealth = Config != null ? Config.MaxHealth : 0f;

            _enemyAnimation.OnAttackImpact += OnAttackImpact;
            _enemyAnimation.OnStrongAttackImpact += OnStrongAttackImpact;
            _enemyAnimation.OnAirAttackImpact += OnAirAttackImpact;
            _enemyAnimation.OnAttackEffectCompleted += OnAttackEffectCompleted;
            _enemyAnimation.OnActionCompleted += OnActionCompleted;

            BuildStateMachine();
            _stateMachine.Enter(EnemyStateId.Rest);

            _enemyService.Register(this);
        }

        private void Start()
        {
            RefreshPlayerReferences();
            PublishHealth();
        }

        private void Update()
        {
            if (IsAlive == false)
            {
                return;
            }

            RefreshPlayerReferences();
            UpdateActionTimeout();
            _stateMachine.Tick();
        }

        private void OnDestroy()
        {
            if (_enemyAnimation != null)
            {
                _enemyAnimation.OnAttackImpact -= OnAttackImpact;
                _enemyAnimation.OnStrongAttackImpact -= OnStrongAttackImpact;
                _enemyAnimation.OnAirAttackImpact -= OnAirAttackImpact;
                _enemyAnimation.OnAttackEffectCompleted -= OnAttackEffectCompleted;
                _enemyAnimation.OnActionCompleted -= OnActionCompleted;
            }

            StopSustainedAttackEffect();
            _enemyService?.Unregister(this);
        }

        public bool ShouldActivateRegularEnemy()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Regular || HasActiveTarget() == false)
            {
                return false;
            }

            if (_enemyModeService.IsPeacefulModeEnabled)
            {
                return false;
            }

            return _isProvoked || DistanceToTarget() <= Config.ChaseRange;
        }

        public bool ShouldActivateBoss()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss || HasActiveTarget() == false)
            {
                return false;
            }

            if (_isProvoked)
            {
                return true;
            }

            return _enemyModeService.IsPeacefulModeEnabled == false && DistanceToTarget() <= Config.ChaseRange;
        }

        public bool ShouldStayInRegularAggression()
        {
            if (HasActiveTarget() == false || _enemyModeService.IsPeacefulModeEnabled)
            {
                return false;
            }

            return _isProvoked || DistanceToTarget() <= GetDisengageRange();
        }

        public bool ShouldStayInBossFight()
        {
            if (HasActiveTarget() == false)
            {
                return false;
            }

            if (_isProvoked == false && _enemyModeService.IsPeacefulModeEnabled)
            {
                return false;
            }

            return _isProvoked || DistanceToTarget() <= GetDisengageRange();
        }

        public bool ShouldFlee()
        {
            return Config.BehaviourType == EnemyBehaviourType.Regular
                && HasActiveTarget()
                && DistanceToTarget() <= GetDisengageRange()
                && GetHealthRatio() <= Config.FleeHealthThreshold;
        }

        public bool ShouldStartEnrage()
        {
            return Config.BehaviourType == EnemyBehaviourType.Boss
                && _isProvoked
                && _isEnraged == false
                && GetHealthRatio() <= Config.EnrageHealthThreshold;
        }

        public bool IsPlayerInAttackRange()
        {
            return HasActiveTarget() && DistanceToTarget() <= Config.AttackRange;
        }

        public bool IsPlayerInStrongAttackRange()
        {
            return HasActiveTarget() && DistanceToTarget() <= Config.StrongAttackRange;
        }

        public bool IsPlayerInAirAttackRange()
        {
            return HasActiveTarget() && DistanceToTarget() <= Config.AirAttackRange;
        }

        public bool CanStartPrimaryAttack()
        {
            return HasActiveTarget()
                && IsActionInProgress == false
                && Time.time >= _nextPrimaryAttackTime
                && CanBossStartOffensiveAction()
                && IsPlayerInAttackRange();
        }

        public bool CanStartStrongAttack()
        {
            return Config.BehaviourType == EnemyBehaviourType.Boss
                && HasActiveTarget()
                && IsActionInProgress == false
                && Time.time >= _nextStrongAttackTime
                && CanBossStartOffensiveAction()
                && IsPlayerInAttackRange() == false
                && IsPlayerInStrongAttackRange();
        }

        public bool CanStartAirAttack()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss
                || HasActiveTarget() == false
                || IsActionInProgress
                || Time.time < _nextAirAttackTime
                || CanBossStartOffensiveAction() == false
                || IsPlayerInAirAttackRange() == false)
            {
                return false;
            }

            return DistanceToTarget() > Config.StrongAttackRange;
        }

        public float GetChaseSpeedMultiplier() => Mathf.Max(0.01f, Config.ChaseSpeedMultiplier);

        public float GetBossChaseSpeedMultiplier()
        {
            float speedMultiplier = Config.ChaseSpeedMultiplier;
            if (_isEnraged)
            {
                speedMultiplier *= Mathf.Max(1f, Config.EnragedSpeedMultiplier);
            }

            return Mathf.Max(0.01f, speedMultiplier);
        }

        public void ClearProvocation()
        {
            _isProvoked = false;
        }

        public void MoveTowardsTarget(float speedMultiplier)
        {
            if (HasActiveTarget() == false)
            {
                StopMovement();
                return;
            }

            if (CanUseAgent() == false)
            {
                return;
            }

            _agent.speed = _baseAgentSpeed * Mathf.Max(0.01f, speedMultiplier);
            _agent.isStopped = false;
            _agent.SetDestination(_playerTransform.position);

            SetMovementAnimation(isMoving: true, isFleeing: false);
        }

        public void MoveAwayFromTarget()
        {
            if (HasActiveTarget() == false)
            {
                StopMovement();
                return;
            }

            if (CanUseAgent() == false)
            {
                return;
            }

            if (_agent.hasPath && Time.time < _nextFleeRepathTime)
            {
                return;
            }

            Vector3 fleeDirection = (transform.position - _playerTransform.position).normalized;
            if (fleeDirection.sqrMagnitude < 0.001f)
            {
                fleeDirection = -transform.forward;
            }

            Vector3 desiredPoint = transform.position + fleeDirection * Config.FleeDistance;
            Vector3 fleeTarget = desiredPoint;

            if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit hit, Config.FleeDistance, NavMesh.AllAreas))
            {
                fleeTarget = hit.position;
            }

            _agent.speed = _baseAgentSpeed * Mathf.Max(0.01f, Config.FleeSpeedMultiplier);
            _agent.isStopped = false;
            _agent.SetDestination(fleeTarget);
            _nextFleeRepathTime = Time.time + Config.FleeRepathInterval;

            SetMovementAnimation(isMoving: true, isFleeing: true);
        }

        public bool HasReachedSafeDistance()
        {
            return HasActiveTarget() == false || DistanceToTarget() >= GetDisengageRange();
        }

        public void StopMovement()
        {
            if (CanUseAgent())
            {
                _agent.isStopped = true;
                if (_agent.hasPath)
                {
                    _agent.ResetPath();
                }

                _agent.speed = _baseAgentSpeed;
            }

            SetMovementAnimation(isMoving: false, isFleeing: false);
        }

        public void LookAtTarget()
        {
            if (HasActiveTarget() == false)
            {
                return;
            }

            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Config.RotationSpeed * Time.deltaTime);
        }

        public void StartAggressionAction()
        {
            StartAction(EnemyActionType.Aggression, Config.AggressionAnimationDuration);
            _enemyAnimation.PlayAggression();
        }

        public void StartPrimaryAttack()
        {
            _nextPrimaryAttackTime = Time.time + Config.AttackCooldown;
            StartAction(EnemyActionType.Attack, Config.AttackAnimationDuration);
            _enemyAnimation.PlayAttack(IsUsingRangedAttack());
        }

        public void StartStrongAttack()
        {
            _nextStrongAttackTime = Time.time + Config.StrongAttackCooldown;
            StartAction(EnemyActionType.StrongAttack, Config.StrongAttackAnimationDuration);
            _enemyAnimation.PlayStrongAttack();
        }

        public void StartAirAttack()
        {
            _nextAirAttackTime = Time.time + Config.AirAttackCooldown;
            StartAction(EnemyActionType.AirAttack, Config.AirAttackAnimationDuration);
            _enemyAnimation.PlayAirAttack();
        }

        public void StartEnrageAction()
        {
            _isEnraged = true;
            StartAction(EnemyActionType.Enrage, Config.EnrageAnimationDuration);
            _enemyAnimation.PlayEnrage();
        }

        public bool ShouldPauseBossOffense()
        {
            return Config.BehaviourType == EnemyBehaviourType.Boss && Time.time < _bossAttackUnlockAt;
        }

        public bool IsBossActionAnimationStillPlaying()
        {
            return IsBossActionAnimationLocked();
        }

        public void TakeDamage(float amount)
        {
            if (IsAlive == false)
            {
                return;
            }

            float actualDamage = Mathf.Max(0f, amount * GetIncomingDamageMultiplier());
            _currentHealth = Mathf.Max(0f, _currentHealth - actualDamage);
            _combatAudioService.PlayEnemyHit();
            PublishHealth();

            if (Config.BehaviourType == EnemyBehaviourType.Boss || _enemyModeService.IsPeacefulModeEnabled == false)
            {
                _isProvoked = true;
            }

            if (_currentHealth <= 0f)
            {
                HandleDeath();
                return;
            }

            if (ShouldPlayHitAnimation())
            {
                _enemyAnimation.PlayHit();
            }
        }

        public void OnPhysicalAttack()
        {
            OnAttackImpact();
        }

        public void OnMagicAttack()
        {
            OnAttackImpact();
        }

        public void OnStrongAttackAnimation()
        {
            OnStrongAttackImpact();
        }

        public void OnAnimationActionCompleted()
        {
            OnActionCompleted();
        }

        public EnemySaveData CaptureSaveData() =>
            new EnemySaveData
            {
                Id = SaveId,
                IsAlive = IsAlive,
                IsProvoked = _isProvoked,
                IsEnraged = _isEnraged,
                CurrentHealth = Mathf.Max(0f, _currentHealth),
                MaxHealth = Config != null ? Config.MaxHealth : 0f,
                RuntimeStateId = CurrentStateId.ToString(),
                Position = Vector3SaveData.FromVector3(transform.position),
                Rotation = Vector3SaveData.FromVector3(transform.eulerAngles)
            };

        public void ApplySaveData(EnemySaveData data)
        {
            if (data == null)
            {
                return;
            }

            SetPositionAndRotation(data.Position.ToVector3(), Quaternion.Euler(data.Rotation.ToVector3()));
            _currentHealth = Mathf.Clamp(data.CurrentHealth, 0f, Config.MaxHealth);
            _isProvoked = data.IsProvoked;
            _isEnraged = data.IsEnraged;
            PublishHealth();

            if (data.IsAlive == false || _currentHealth <= 0f)
            {
                _enemyService.MarkDead(this);
                Destroy(gameObject);
                return;
            }

            EnableAllColliders(true);
            ResetActionState();
            EnterRuntimeStateAfterLoad();
        }

        private void BuildStateMachine()
        {
            _stateMachine = new EnemyStateMachine();
            _stateMachine.AddState(new EnemyRestState(this, _stateMachine));
            _stateMachine.AddState(new EnemyAttackState(this, _stateMachine, EnemyActionType.Attack, GetPostAttackStateId()));

            if (Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                _stateMachine.AddState(new BossEnemyAggressionState(this, _stateMachine));
                _stateMachine.AddState(new BossEnemyChaseState(this, _stateMachine));
                _stateMachine.AddState(new EnemyAttackState(this, _stateMachine, EnemyActionType.StrongAttack, EnemyStateId.Chase));
                _stateMachine.AddState(new BossEnemyEnrageState(this, _stateMachine));
                _stateMachine.AddState(new BossEnemyAirAttackState(this, _stateMachine));
            }
            else
            {
                _stateMachine.AddState(new RegularEnemyAggressionState(this, _stateMachine));
                _stateMachine.AddState(new EnemyFleeState(this, _stateMachine));
            }
        }

        private EnemyStateId GetPostAttackStateId() =>
            Config.BehaviourType == EnemyBehaviourType.Boss
                ? EnemyStateId.Chase
                : EnemyStateId.Aggression;

        private void EnterRuntimeStateAfterLoad()
        {
            if (ShouldFlee())
            {
                _stateMachine.Enter(EnemyStateId.Flee);
                return;
            }

            if (_isProvoked)
            {
                _stateMachine.Enter(Config.BehaviourType == EnemyBehaviourType.Boss
                    ? EnemyStateId.Chase
                    : EnemyStateId.Aggression);
                return;
            }

            _stateMachine.Enter(EnemyStateId.Rest);
        }

        private void RefreshPlayerReferences()
        {
            if (_playerTransform != null && _playerDamageable != null)
            {
                return;
            }

            _playerTransform = _playerService.PlayerTransform;
            _playerDamageable = _playerService.PlayerObject != null
                ? _playerService.PlayerObject.GetComponent<IDamageable>()
                : null;
        }

        private void PublishHealth()
        {
            _healthFeedback?.OnHealthChanged(_currentHealth, Config.MaxHealth);
        }

        private void HandleDeath()
        {
            ResetActionState();
            StopMovement();
            _enemyAnimation.PlayDeath();
            EnableAllColliders(false);
            _enemyService.MarkDead(this);
            Destroy(gameObject, 3f);
        }

        private void EnableAllColliders(bool isEnabled)
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = isEnabled;
            }
        }

        private void StartAction(EnemyActionType actionType, float fallbackDuration)
        {
            _currentAction = actionType;
            _actionImpactConsumed = false;
            _actionTimeoutAt = Time.time + Mathf.Max(0.1f, fallbackDuration);
        }

        private void ResetActionState()
        {
            StopSustainedAttackEffect();
            _currentAction = EnemyActionType.None;
            _actionImpactConsumed = false;
            _actionTimeoutAt = 0f;
        }

        private void UpdateActionTimeout()
        {
            if (IsActionInProgress == false || Time.time < _actionTimeoutAt)
            {
                return;
            }

            if (IsBossActionAnimationLocked())
            {
                return;
            }

            ResetActionState();
        }

        private void OnActionCompleted()
        {
            ApplyBossPostActionDelay();

            if (_currentAction == EnemyActionType.AirAttack
                && _enemyAnimation.IsCurrentStateOrTransitioningTo("Land") == false)
            {
                return;
            }

            ResetActionState();
        }

        private void OnAttackEffectCompleted()
        {
            if (_currentAction != EnemyActionType.StrongAttack
                && _currentAction != EnemyActionType.AirAttack)
            {
                return;
            }

            StopSustainedAttackEffect();
        }

        private void OnAttackImpact()
        {
            if (_actionImpactConsumed)
            {
                return;
            }

            if (_currentAction == EnemyActionType.Attack)
            {
                ExecutePrimaryAttack();
                _actionImpactConsumed = true;
            }
        }

        private void OnStrongAttackImpact()
        {
            if (_actionImpactConsumed || _currentAction != EnemyActionType.StrongAttack)
            {
                return;
            }

            ExecuteStrongAttack();
            _actionImpactConsumed = true;
        }

        private void OnAirAttackImpact()
        {
            if (_actionImpactConsumed || _currentAction != EnemyActionType.AirAttack)
            {
                return;
            }

            ExecuteAirAttack();
            _actionImpactConsumed = true;
        }

        private void ExecutePrimaryAttack()
        {
            float damage = Config.Damage * GetCurrentDamageMultiplier();
            if (IsUsingRangedAttack())
            {
                ExecuteRangedAttack(damage, Config.ProjectileSpeed, 1, 0f);
                return;
            }

            ExecuteMeleeAttack(damage, Config.HitRadius);
        }

        private void ExecuteStrongAttack()
        {
            float damage = Config.Damage * Config.StrongAttackDamageMultiplier * GetCurrentDamageMultiplier();
            EnemyAttackDeliveryType deliveryType = ResolveStrongAttackDeliveryType();

            if (deliveryType == EnemyAttackDeliveryType.SustainedEffect)
            {
                StartSustainedAttackEffect(
                    damagePerSecond: damage,
                    radius: Config.StrongAttackEffectRadius,
                    emissionInterval: Config.StrongAttackDamageTickInterval,
                    projectileSpeed: Config.ProjectileSpeed * Config.StrongAttackProjectileSpeedMultiplier);
                return;
            }

            if (deliveryType == EnemyAttackDeliveryType.Ranged && Config.ProjectilePrefab != null)
            {
                ExecuteRangedAttack(
                    damage,
                    Config.ProjectileSpeed * Config.StrongAttackProjectileSpeedMultiplier,
                    Mathf.Max(1, Config.StrongAttackProjectileCount),
                    Config.StrongAttackProjectileSpreadAngle);
                return;
            }

            ExecuteMeleeAttack(damage, Config.HitRadius * Config.StrongAttackHitRadiusMultiplier);
        }

        private void ExecuteAirAttack()
        {
            float damage = Config.Damage * Config.AirAttackDamageMultiplier * GetCurrentDamageMultiplier();
            StartSustainedAttackEffect(
                damagePerSecond: damage,
                radius: Config.AirAttackEffectRadius,
                emissionInterval: Config.AirAttackDamageTickInterval,
                projectileSpeed: Config.ProjectileSpeed * Config.StrongAttackProjectileSpeedMultiplier);
        }

        private void ExecuteMeleeAttack(float damage, float radius)
        {
            _combatAudioService.PlayEnemyMeleeAttack();

            Vector3 attackPosition = _meleeAttackPoint != null ? _meleeAttackPoint.position : transform.position;
            Collider[] hitColliders = Physics.OverlapSphere(attackPosition, radius, _playerLayer);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<IDamageable>(out IDamageable victim) && victim.IsAlive)
                {
                    victim.TakeDamage(damage);
                    break;
                }
            }
        }

        private void ExecuteRangedAttack(
            float damage,
            float projectileSpeed,
            int projectileCount,
            float spreadAngle)
        {
            _combatAudioService.PlayEnemyMagicAttack();

            if (Config.ProjectilePrefab == null)
            {
                return;
            }

            Transform origin = _shootPoint != null ? _shootPoint : transform;
            int safeProjectileCount = Mathf.Max(1, projectileCount);
            float startAngle = -spreadAngle * (safeProjectileCount - 1) * 0.5f;

            for (int i = 0; i < safeProjectileCount; i++)
            {
                float angleOffset = startAngle + spreadAngle * i;
                Quaternion rotation = origin.rotation * Quaternion.Euler(0f, angleOffset, 0f);
                GameObject projectile = _gameObjectFactory.Instantiate(
                    Config.ProjectilePrefab,
                    origin.position,
                    rotation);

                if (projectile.TryGetComponent(out MagicProjectile magicProjectile))
                {
                    magicProjectile.Setup(damage, projectileSpeed);
                }
            }
        }

        private float GetCurrentDamageMultiplier() =>
            _isEnraged ? Mathf.Max(1f, Config.EnragedDamageMultiplier) : 1f;

        private float GetDisengageRange() =>
            Mathf.Max(Config.ChaseRange, Config.DisengageRange);

        private float GetHealthRatio() =>
            Config.MaxHealth <= 0f ? 0f : _currentHealth / Config.MaxHealth;

        private float DistanceToTarget()
        {
            if (HasActiveTarget() == false)
            {
                return float.MaxValue;
            }

            return Vector3.Distance(transform.position, _playerTransform.position);
        }

        private bool HasActiveTarget()
        {
            return _playerTransform != null && (_playerDamageable == null || _playerDamageable.IsAlive);
        }

        private bool CanUseAgent()
        {
            return _agent != null && _agent.enabled && _agent.isOnNavMesh;
        }

        private bool IsUsingRangedAttack()
        {
            return Config.Type == EnemyType.Ranged && Config.ProjectilePrefab != null;
        }

        private bool ShouldPlayHitAnimation()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return true;
            }

            return CurrentStateId == EnemyStateId.Chase && IsActionInProgress == false;
        }

        private bool IsBossActionAnimationLocked()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss || _enemyAnimation == null)
            {
                return false;
            }

            return _currentAction switch
            {
                EnemyActionType.Aggression => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Scream"),
                EnemyActionType.Attack => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Claw Attack"),
                EnemyActionType.StrongAttack => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Flame Attack"),
                EnemyActionType.AirAttack => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Take Off", "Fly Flame Attack", "Land"),
                EnemyActionType.Enrage => _enemyAnimation.IsAnyCurrentStateOrTransitioningTo("Defend"),
                _ => false
            };
        }

        private float GetIncomingDamageMultiplier()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return 1f;
            }

            if (_currentAction == EnemyActionType.Enrage || CurrentStateId == EnemyStateId.Enrage)
            {
                return Mathf.Clamp01(Config.DefendDamageTakenMultiplier);
            }

            return 1f;
        }

        private void ApplyBossPostActionDelay()
        {
            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return;
            }

            switch (_currentAction)
            {
                case EnemyActionType.Aggression:
                    _bossAttackUnlockAt = Time.time + Mathf.Max(0f, Config.AggressionPostActionDelay);
                    break;
                case EnemyActionType.Enrage:
                    _bossAttackUnlockAt = Time.time + Mathf.Max(0f, Config.EnragePostActionDelay);
                    break;
            }
        }

        private bool CanBossStartOffensiveAction()
        {
            return Config.BehaviourType != EnemyBehaviourType.Boss || Time.time >= _bossAttackUnlockAt;
        }

        private EnemyAttackDeliveryType ResolveStrongAttackDeliveryType()
        {
            if (Config.StrongAttackDeliveryType != EnemyAttackDeliveryType.Auto)
            {
                return Config.StrongAttackDeliveryType;
            }

            if (Config.SustainedAttackEffectPrefab != null)
            {
                return EnemyAttackDeliveryType.SustainedEffect;
            }

            return Config.ProjectilePrefab != null
                ? EnemyAttackDeliveryType.Ranged
                : EnemyAttackDeliveryType.Melee;
        }

        private GameObject GetSustainedProjectilePrefab()
        {
            return Config.SustainedAttackProjectilePrefab != null
                ? Config.SustainedAttackProjectilePrefab
                : Config.ProjectilePrefab;
        }

        private void SetMovementAnimation(bool isMoving, bool isFleeing)
        {
            _enemyAnimation.SetIsWalking(false);
            _enemyAnimation.SetIsRunning(isMoving);
            _enemyAnimation.SetIsFleeing(isFleeing);
            _enemyAnimation.SetRunSpeed(isMoving ? 1f : 0f);

            if (IsActionInProgress == false)
            {
                _enemyAnimation.SyncLocomotionState(isMoving);
            }
        }

        private void StartSustainedAttackEffect(float damagePerSecond, float radius, float emissionInterval, float projectileSpeed)
        {
            StopSustainedAttackEffect();

            Transform origin = _shootPoint != null ? _shootPoint : transform;
            if (origin == null)
            {
                return;
            }

            if (Config.SustainedAttackEffectPrefab != null)
            {
                _activeSustainedAttackEffect = _gameObjectFactory.Instantiate(
                    Config.SustainedAttackEffectPrefab,
                    origin.position,
                    origin.rotation,
                    origin);
            }
            else
            {
                _activeSustainedAttackEffect = new GameObject("BossSustainedAttackEffect");
                _activeSustainedAttackEffect.transform.SetParent(origin, false);
                _activeSustainedAttackEffect.transform.localPosition = Vector3.zero;
                _activeSustainedAttackEffect.transform.localRotation = Quaternion.identity;
            }

            GameObject sustainedProjectilePrefab = GetSustainedProjectilePrefab();
            if (sustainedProjectilePrefab != null)
            {
                if (_activeSustainedAttackEffect.TryGetComponent(out EnemyPersistentDamageEffect damageEffect))
                {
                    Destroy(damageEffect);
                }

                if (_activeSustainedAttackEffect.TryGetComponent(out EnemyProjectileEmitter projectileEmitter) == false)
                {
                    projectileEmitter = _activeSustainedAttackEffect.AddComponent<EnemyProjectileEmitter>();
                }

                projectileEmitter.Setup(
                    _gameObjectFactory,
                    sustainedProjectilePrefab,
                    _playerTransform,
                    damagePerSecond,
                    projectileSpeed,
                    emissionInterval,
                    visualProjectileLifetime: Config.SustainedAttackProjectileLifetime,
                    visualRotationOffset: Config.SustainedAttackProjectileRotationOffset);
            }
            else
            {
                if (_activeSustainedAttackEffect.TryGetComponent(out EnemyProjectileEmitter projectileEmitter))
                {
                    Destroy(projectileEmitter);
                }

                EnemyPersistentDamageEffect damageEffect;
                if (_activeSustainedAttackEffect.TryGetComponent(out damageEffect) == false)
                {
                    damageEffect = _activeSustainedAttackEffect.AddComponent<EnemyPersistentDamageEffect>();
                }

                damageEffect.Setup(damagePerSecond, emissionInterval, radius, _playerLayer);
            }

            _combatAudioService.PlayEnemyMagicAttack();
        }

        private void StopSustainedAttackEffect()
        {
            if (_activeSustainedAttackEffect == null)
            {
                return;
            }

            Destroy(_activeSustainedAttackEffect);
            _activeSustainedAttackEffect = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (Config == null)
            {
                return;
            }

            DrawBossAttackPreviewGizmos(drawAllRanges: true);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false || Config == null || Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return;
            }

            if (_currentAction != EnemyActionType.Attack
                && _currentAction != EnemyActionType.StrongAttack
                && _currentAction != EnemyActionType.AirAttack)
            {
                return;
            }

            DrawBossAttackPreviewGizmos(drawAllRanges: false);
        }

        private void DrawBossAttackPreviewGizmos(bool drawAllRanges)
        {
            if (Config == null)
            {
                return;
            }

            if (drawAllRanges)
            {
                DrawRangeGizmo(transform.position, Config.AttackRange, new Color(1f, 0.92f, 0.16f, 0.8f));
                DrawRangeGizmo(transform.position, Config.StrongAttackRange, new Color(1f, 0.45f, 0.05f, 0.8f));

                if (Config.BehaviourType == EnemyBehaviourType.Boss)
                {
                    DrawRangeGizmo(transform.position, Config.AirAttackRange, new Color(0.35f, 0.85f, 1f, 0.8f));
                }
            }

            bool drawMeleeHit = drawAllRanges
                || _currentAction == EnemyActionType.Attack;

            if (drawMeleeHit)
            {
                Vector3 meleePoint = _meleeAttackPoint != null ? _meleeAttackPoint.position : transform.position;
                Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.85f);
                Gizmos.DrawWireSphere(meleePoint, Config.HitRadius);
            }

            if (Config.BehaviourType != EnemyBehaviourType.Boss)
            {
                return;
            }

            Vector3 shootPoint = _shootPoint != null ? _shootPoint.position : transform.position;

            if (drawAllRanges || _currentAction == EnemyActionType.StrongAttack)
            {
                Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.85f);
                Gizmos.DrawWireSphere(shootPoint, Config.StrongAttackEffectRadius);
            }

            if (drawAllRanges || _currentAction == EnemyActionType.AirAttack)
            {
                Gizmos.color = new Color(0.35f, 0.85f, 1f, 0.85f);
                Gizmos.DrawWireSphere(shootPoint, Config.AirAttackEffectRadius);
            }
        }

        private static void DrawRangeGizmo(Vector3 origin, float radius, Color color)
        {
            if (radius <= 0f)
            {
                return;
            }

            Gizmos.color = color;
            Gizmos.DrawWireSphere(origin, radius);
        }

        private void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (CanUseAgent())
            {
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
                else
                {
                    transform.position = position;
                }
            }
            else
            {
                transform.position = position;
            }

            transform.rotation = rotation;
        }
    }
}
