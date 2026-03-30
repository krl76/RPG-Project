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
    /// Окно паузы с управлением текущей игровой сессией.
    /// </summary>
    public sealed class PauseWindow : WindowBase, IPauseView
    {
        public override WindowID Id => WindowID.Pause;
        public override bool IsPopup => true;

        [SerializeField] private Button _goToGameButton;
        [SerializeField] private Button _saveGameButton;
        [SerializeField] private Button _loadGameButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitToMainMenuButton;

        public event System.Action ResumeRequested;
        public event System.Action SaveRequested;
        public event System.Action LoadRequested;
        public event System.Action SettingsRequested;
        public event System.Action ExitToMainMenuRequested;

        private PauseWindowController _controller;

        [Inject]
        private void Construct(PauseWindowController controller)
        {
            _controller = controller;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            _goToGameButton.onClick.AddListener(RaiseResumeRequested);
            _saveGameButton.onClick.AddListener(RaiseSaveRequested);
            _loadGameButton.onClick.AddListener(RaiseLoadRequested);
            _settingsButton.onClick.AddListener(RaiseSettingsRequested);
            _exitToMainMenuButton.onClick.AddListener(RaiseExitToMainMenuRequested);
            _controller.Attach(this);
        }

        public override void OnClose()
        {
            _controller.Detach();
            _goToGameButton.onClick.RemoveListener(RaiseResumeRequested);
            _saveGameButton.onClick.RemoveListener(RaiseSaveRequested);
            _loadGameButton.onClick.RemoveListener(RaiseLoadRequested);
            _settingsButton.onClick.RemoveListener(RaiseSettingsRequested);
            _exitToMainMenuButton.onClick.RemoveListener(RaiseExitToMainMenuRequested);

            base.OnClose();
        }

        private void RaiseResumeRequested() => ResumeRequested?.Invoke();
        private void RaiseSaveRequested() => SaveRequested?.Invoke();
        private void RaiseLoadRequested() => LoadRequested?.Invoke();
        private void RaiseSettingsRequested() => SettingsRequested?.Invoke();
        private void RaiseExitToMainMenuRequested() => ExitToMainMenuRequested?.Invoke();
    }
}
