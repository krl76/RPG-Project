using Features.Player;
using Infrastructure.Services.Events;
using Infrastructure.Services.Player;
using Infrastructure.Services.Player.Input;
using UI.MVC.Views;

namespace UI.MVC.Controllers
{
    public sealed class HUDWindowController : IPlayerHealthSubscriber, IPlayerMagicSubscriber
    {
        private readonly IPlayerService _playerService;
        private readonly IFightInputService _fightInputService;

        private IHUDView _view;

        public HUDWindowController(IPlayerService playerService, IFightInputService fightInputService)
        {
            _playerService = playerService;
            _fightInputService = fightInputService;
        }

        public void Attach(IHUDView view)
        {
            Detach();

            _view = view;
            EventBus.Subscribe(this);
            SyncView();
        }

        public void Detach()
        {
            if (_view == null)
            {
                return;
            }

            EventBus.Unsubscribe(this);
            _view = null;
        }

        public void OnPlayerHealthChanged(float currentHealth, float maxHealth)
        {
            _view?.SetHealth(currentHealth, maxHealth);
        }

        public void OnPlayerDied()
        {
        }

        public void OnMagicUsed(float remainingTime, float totalDuration)
        {
            _view?.SetMagicCooldown(remainingTime, totalDuration);
        }

        public void OnMagicReady()
        {
            _view?.CompleteMagicCooldown();
        }

        private void SyncView()
        {
            if (_view == null)
            {
                return;
            }

            var playerHealth = _playerService.PlayerObject != null
                ? _playerService.PlayerObject.GetComponent<PlayerHealth>()
                : null;

            if (playerHealth != null)
            {
                _view.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (_fightInputService.MagicCooldownRemaining > 0f && _fightInputService.MagicCooldownDuration > 0f)
            {
                _view.SetMagicCooldown(
                    _fightInputService.MagicCooldownRemaining,
                    _fightInputService.MagicCooldownDuration);
            }
            else
            {
                _view.CompleteMagicCooldown();
            }
        }
    }
}
