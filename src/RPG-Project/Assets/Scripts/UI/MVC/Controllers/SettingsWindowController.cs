using Infrastructure.Services.Audio;
using Infrastructure.Services.Player.Input;
using Infrastructure.Services.UI;
using UI.MVC.Views;

namespace UI.MVC.Controllers
{
    public sealed class SettingsWindowController
    {
        private readonly IWindowService _windowService;
        private readonly IAudioService _audioService;
        private readonly IInputBindingService _inputBindingService;

        private ISettingsView _view;

        public SettingsWindowController(
            IWindowService windowService,
            IAudioService audioService,
            IInputBindingService inputBindingService)
        {
            _windowService = windowService;
            _audioService = audioService;
            _inputBindingService = inputBindingService;
        }

        public void Attach(ISettingsView view)
        {
            Detach();

            _view = view;
            _view.CloseRequested += OnCloseRequested;
            _view.MasterVolumeChanged += OnMasterVolumeChanged;
            _view.MusicVolumeChanged += OnMusicVolumeChanged;
            _view.EffectsVolumeChanged += OnEffectsVolumeChanged;
            _view.RebindRequested += OnRebindRequested;

            _view.SetVolumes(_audioService.MasterVolume, _audioService.MusicVolume, _audioService.EffectsVolume);
            RefreshBindingTexts();
        }

        public void Detach()
        {
            if (_view == null)
            {
                return;
            }

            ISettingsView currentView = _view;
            _view = null;

            currentView.CloseRequested -= OnCloseRequested;
            currentView.MasterVolumeChanged -= OnMasterVolumeChanged;
            currentView.MusicVolumeChanged -= OnMusicVolumeChanged;
            currentView.EffectsVolumeChanged -= OnEffectsVolumeChanged;
            currentView.RebindRequested -= OnRebindRequested;

            _inputBindingService.CancelRebind();
        }

        private void OnCloseRequested()
        {
            _windowService.Close(WindowID.Settings);
        }

        private void OnMasterVolumeChanged(float value) => _audioService.SetMasterVolume(value);
        private void OnMusicVolumeChanged(float value) => _audioService.SetMusicVolume(value);
        private void OnEffectsVolumeChanged(float value) => _audioService.SetEffectsVolume(value);

        private void OnRebindRequested(InputBindingKey bindingKey)
        {
            if (_view == null)
            {
                return;
            }

            if (_inputBindingService.StartRebind(bindingKey, OnRebindFinished, OnRebindFinished) == false)
            {
                return;
            }

            _view.SetRebindButtonsInteractable(false);
            _view.ShowRebindPrompt(bindingKey);
        }

        private void OnRebindFinished()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetRebindButtonsInteractable(true);
            RefreshBindingTexts();
        }

        private void RefreshBindingTexts()
        {
            if (_view == null)
            {
                return;
            }

            _view.SetBindingDisplay(InputBindingKey.Jump, _inputBindingService.GetBindingDisplay(InputBindingKey.Jump));
            _view.SetBindingDisplay(InputBindingKey.Sprint, _inputBindingService.GetBindingDisplay(InputBindingKey.Sprint));
            _view.SetBindingDisplay(InputBindingKey.SwordAttack, _inputBindingService.GetBindingDisplay(InputBindingKey.SwordAttack));
            _view.SetBindingDisplay(InputBindingKey.MagicAttack, _inputBindingService.GetBindingDisplay(InputBindingKey.MagicAttack));
        }
    }
}
