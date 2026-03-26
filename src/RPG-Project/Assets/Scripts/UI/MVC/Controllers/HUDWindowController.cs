using Infrastructure.Services.Events;
using UI.MVC.Views;

namespace UI.MVC.Controllers
{
    public sealed class HUDWindowController : IPlayerHealthSubscriber, IPlayerMagicSubscriber
    {
        private IHUDView _view;

        public void Attach(IHUDView view)
        {
            Detach();

            _view = view;
            EventBus.Subscribe(this);
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

        public void OnMagicUsed(float cooldownDuration)
        {
            _view?.StartMagicCooldown(cooldownDuration);
        }

        public void OnMagicReady()
        {
            _view?.CompleteMagicCooldown();
        }
    }
}
