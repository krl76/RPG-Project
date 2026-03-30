using Infrastructure.Services.UI;
using UI.Base;
using UI.MVC.Controllers;
using UI.MVC.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    /// <summary>
    /// Окно главного меню с основными действиями игрока.
    /// </summary>
    public sealed class MainMenuWindow : WindowBase, IMainMenuView
    {
        public override WindowID Id => WindowID.MainMenu;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        public event System.Action PlayRequested;
        public event System.Action SettingsRequested;
        public event System.Action ExitRequested;

        private MainMenuWindowController _controller;

        [Inject]
        private void Construct(MainMenuWindowController controller)
        {
            _controller = controller;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            _startButton.onClick.AddListener(RaisePlayRequested);
            _settingsButton.onClick.AddListener(RaiseSettingsRequested);
            _exitButton.onClick.AddListener(RaiseExitRequested);
            _controller.Attach(this);
        }

        public override void OnClose()
        {
            _controller.Detach();
            _startButton.onClick.RemoveListener(RaisePlayRequested);
            _settingsButton.onClick.RemoveListener(RaiseSettingsRequested);
            _exitButton.onClick.RemoveListener(RaiseExitRequested);

            base.OnClose();
        }

        private void RaisePlayRequested() => PlayRequested?.Invoke();
        private void RaiseSettingsRequested() => SettingsRequested?.Invoke();
        private void RaiseExitRequested() => ExitRequested?.Invoke();
    }
}
