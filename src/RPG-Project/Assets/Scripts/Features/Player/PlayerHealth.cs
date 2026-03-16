using System;
using Features.Combat;
using Infrastructure.Providers.Configs;
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
        
        [Inject]
        private void Construct(IConfigDataProvider configDataProvider, IPlayerAnimatorService playerAnimatorService)
        {
            _configDataProvider = configDataProvider;
            _playerAnimatorService = playerAnimatorService;
        }

        private void Start()
        {
            _maxHealth = _configDataProvider.GetPlayerStatsConfig().InitialHealth;
            _currentHealth = _maxHealth; //TODO: подгружать из сохранения
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            
            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            EventBus.RaiseEvent<IPlayerHealthSubscriber>(sub =>
                sub.OnPlayerHealthChanged(_currentHealth, _maxHealth));

            if (_currentHealth > 0)
            {
                _playerAnimatorService.TriggerHit();
            }
            else
            {
                _playerAnimatorService.TriggerDeath();
                EventBus.RaiseEvent<IPlayerHealthSubscriber>(sub => sub.OnPlayerDied());
                
                EventBus.RaiseEvent<IGameStateSubscriber>(sub => sub.OnGameOver());
            }
        }
    }
}