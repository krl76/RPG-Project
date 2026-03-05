using System;
using Features.Combat;
using Infrastructure.Providers.Configs;
using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public event Action OnHealthChanged;
        public bool IsAlive => _currentHealth > 0;
        
        private float _currentHealth;

        private IConfigDataProvider _configDataProvider;
        private IPlayerAnimatorService _playerAnimatorService;


        [Inject]
        private void Construct(IConfigDataProvider configDataProvider, IPlayerAnimatorService playerAnimatorService)
        {
            _configDataProvider = configDataProvider;
            _playerAnimatorService = playerAnimatorService;

            _currentHealth = _configDataProvider.GetPlayerStatsConfig().InitialHealth;
        }

        public void TakeDamage(float amount)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (_currentHealth > 0)
            {
                _playerAnimatorService.TriggerHit();
            }
            else
            {
                _playerAnimatorService.TriggerDeath();
            }
            
            OnHealthChanged?.Invoke();
        }
    }
}