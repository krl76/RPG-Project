using Core.Gameplay.Pause;
using Infrastructure.Services.UI;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public sealed class PauseWindow : WindowBase
    {
        public override WindowID Id => WindowID.Pause;
        public override bool IsPopup => true;

        [SerializeField] private Button _goToGameButton;
        [SerializeField] private Button _saveGameButton;
        [SerializeField] private Button _loadGameButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitToMainMenuButton;

        private GameplayPauseController _gameplayPauseController;

        [Inject]
        private void Construct(GameplayPauseController gameplayPauseController)
        {
            _gameplayPauseController = gameplayPauseController;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            _goToGameButton.onClick.AddListener(ResumeGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitToMainMenuButton.onClick.AddListener(ExitToMainMenu);
        }

        public override void OnClose()
        {
            _goToGameButton.onClick.RemoveListener(ResumeGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitToMainMenuButton.onClick.RemoveListener(ExitToMainMenu);

            base.OnClose();
        }

        private void ResumeGame()
        {
            _gameplayPauseController.Resume();
        }

        private void OpenSettings()
        {
            _gameplayPauseController.OpenSettings();
        }

        private void ExitToMainMenu()
        {
            _gameplayPauseController.ExitToMainMenu();
        }
    }
}
