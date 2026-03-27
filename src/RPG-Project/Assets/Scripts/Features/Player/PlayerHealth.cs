using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
using Core.Gameplay.Save.Data;
using Features.Combat;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Events;
using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public bool IsAlive => _currentHealth > 0;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        private float _currentHealth;
        private float _maxHealth;

        private IConfigDataProvider _configDataProvider;
        private IPlayerAnimatorService _playerAnimatorService;
        private IGameStateMachine _gameStateMachine;
        private ICombatAudioService _combatAudioService;

        [Inject]
        private void Construct(
            IConfigDataProvider configDataProvider,
            IPlayerAnimatorService playerAnimatorService,
            IGameStateMachine gameStateMachine,
            ICombatAudioService combatAudioService)
        {
            _configDataProvider = configDataProvider;
            _playerAnimatorService = playerAnimatorService;
            _gameStateMachine = gameStateMachine;
            _combatAudioService = combatAudioService;
        }

        private void Awake()
        {
            _maxHealth = _configDataProvider.GetPlayerStatsConfig().InitialHealth;
            _currentHealth = _maxHealth;
            PublishHealthState();
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _combatAudioService.PlayPlayerHit();

            PublishHealthState();

            if (_currentHealth > 0)
            {
                _playerAnimatorService.TriggerHit();
                return;
            }

            _playerAnimatorService.TriggerDeath();
            EventBus.RaiseEvent<IPlayerHealthSubscriber>(sub => sub.OnPlayerDied());
            _gameStateMachine.Enter<GameOverState>().Forget();
        }

        public void ApplySaveData(PlayerSaveData data)
        {
            if (data == null)
            {
                return;
            }

            _maxHealth = data.MaxHealth > 0f
                ? data.MaxHealth
                : _configDataProvider.GetPlayerStatsConfig().InitialHealth;
            _currentHealth = Mathf.Clamp(data.CurrentHealth, 0f, _maxHealth);
            PublishHealthState();
        }

        private void PublishHealthState()
        {
            EventBus.RaiseEvent<IPlayerHealthSubscriber>(sub =>
                sub.OnPlayerHealthChanged(_currentHealth, _maxHealth));
        }
    }
}
