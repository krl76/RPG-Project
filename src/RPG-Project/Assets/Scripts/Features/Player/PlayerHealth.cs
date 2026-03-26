using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
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

        private void Start()
        {
            _maxHealth = _configDataProvider.GetPlayerStatsConfig().InitialHealth;
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _combatAudioService.PlayPlayerHit();

            EventBus.RaiseEvent<IPlayerHealthSubscriber>(sub =>
                sub.OnPlayerHealthChanged(_currentHealth, _maxHealth));

            if (_currentHealth > 0)
            {
                _playerAnimatorService.TriggerHit();
                return;
            }

            _playerAnimatorService.TriggerDeath();
            EventBus.RaiseEvent<IPlayerHealthSubscriber>(sub => sub.OnPlayerDied());
            _gameStateMachine.Enter<GameOverState>().Forget();
        }
    }
}
