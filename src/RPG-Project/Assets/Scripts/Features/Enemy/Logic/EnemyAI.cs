using Core.Gameplay.Save;
using Core.Gameplay.Save.Data;
using Data.Configs;
using Features.Combat;
using Features.Enemy.States;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Enemy;
using Infrastructure.Services.Gameplay;
using Infrastructure.Services.Player;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Features.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(EnemyAnimation))]
    public partial class EnemyAI : MonoBehaviour, IDamageable
    {
        public bool IsAlive => _currentHealth > 0f;
        public float CurrentHealth => _currentHealth;
        public string SaveId => string.IsNullOrWhiteSpace(_saveId) ? SceneObjectSaveId.Build(transform) : _saveId;
        public EnemyStateId CurrentStateId => _stateMachine?.CurrentStateId ?? EnemyStateId.None;
        [field: SerializeField] public EnemyConfig Config { get; private set; }

        [Header("References")]
        [SerializeField] private Transform _meleeAttackPoint;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private LayerMask _playerLayer;

        private float _currentHealth;
        private bool _isProvoked;
        private bool _isEnraged;
        private string _saveId;

        private Transform _playerTransform;
        private IDamageable _playerDamageable;

        private EnemyAnimation _enemyAnimation;
        private EnemyStateMachine _stateMachine;
        private IHealthFeedback _healthFeedback;
        private IGameObjectFactory _gameObjectFactory;
        private IEnemyService _enemyService;
        private IEnemyModeService _enemyModeService;
        private IPlayerService _playerService;
        private IEffectsAudioService _effectsAudioService;
        private IGameplayProgressService _gameplayProgressService;

        [Inject]
        private void Construct(
            IPlayerService playerService,
            IGameObjectFactory gameObjectFactory,
            IEnemyService enemyService,
            IEnemyModeService enemyModeService,
            IEffectsAudioService effectsAudioService,
            IGameplayProgressService gameplayProgressService)
        {
            _playerService = playerService;
            _gameObjectFactory = gameObjectFactory;
            _enemyService = enemyService;
            _enemyModeService = enemyModeService;
            _effectsAudioService = effectsAudioService;
            _gameplayProgressService = gameplayProgressService;
        }

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
            _stateAudioSource = _effectsAudioService?.CreateConfiguredSource(transform, "[EnemyStateAudio]");
            _loopingStateAudioSource = _effectsAudioService?.CreateConfiguredSource(transform, "[EnemyLoopingStateAudio]");

            _baseAgentSpeed = _agent.speed;
            _currentHealth = Config.MaxHealth;
            InitializeVisualVariation();

            _enemyAnimation.OnAttackImpact += OnAttackImpact;
            _enemyAnimation.OnStrongAttackImpact += OnStrongAttackImpact;
            _enemyAnimation.OnAirAttackImpact += OnAirAttackImpact;
            _enemyAnimation.OnAttackEffectCompleted += OnAttackEffectCompleted;
            _enemyAnimation.OnActionCompleted += OnActionCompleted;

            BuildStateMachine();
            _stateMachine.Enter(EnemyStateId.Rest);
        }

        private void Start()
        {
            RefreshPlayerReferences();
            PublishHealth();
            _enemyService.Register(this);
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
            RefreshLocomotionAnimation();
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
            StopStateAudio();
            StopLoopingStateAudio();
            DestroyActiveWeaponEffect();
            _enemyService?.Unregister(this);
        }

        public void TakeDamage(float amount)
        {
            if (IsAlive == false)
            {
                return;
            }

            float actualDamage = Mathf.Max(0f, amount * GetIncomingDamageMultiplier());
            _currentHealth = Mathf.Max(0f, _currentHealth - actualDamage);
            PlayAudioCue(Config.HitSound, 0);
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
                ConfigId = Config != null ? Config.Id : 0,
                IsAlive = IsAlive,
                IsProvoked = _isProvoked,
                IsEnraged = _isEnraged,
                HasSelectedRegularVariation = _selectedRegularVariationIndex >= 0,
                SelectedRegularVariationIndex = _selectedRegularVariationIndex,
                HasSelectedBossElement = _selectedBossElementIndex >= 0,
                SelectedBossElementIndex = _selectedBossElementIndex,
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

            SetSaveId(data.Id);
            SetPositionAndRotation(data.Position.ToVector3(), Quaternion.Euler(data.Rotation.ToVector3()));
            _currentHealth = Mathf.Clamp(data.CurrentHealth, 0f, Config.MaxHealth);
            _isProvoked = data.IsProvoked;
            _isEnraged = data.IsEnraged;
            RestoreVisualVariation(data);
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

        public void SetSaveId(string saveId)
        {
            _saveId = string.IsNullOrWhiteSpace(saveId) ? _saveId : saveId;
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
            _gameplayProgressService?.RegisterEnemyKill(Config);
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
    }
}
