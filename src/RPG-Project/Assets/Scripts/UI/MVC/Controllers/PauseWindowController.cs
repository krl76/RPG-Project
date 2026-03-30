using Core.Gameplay.Pause;
using UI.MVC.Views;

namespace UI.MVC.Controllers
{
    /// <summary>
    /// Контроллер окна паузы и связанных действий.
    /// </summary>
    public sealed class PauseWindowController
    {
        private readonly GameplayPauseController _gameplayPauseController;

        private IPauseView _view;

        public PauseWindowController(GameplayPauseController gameplayPauseController)
        {
            _gameplayPauseController = gameplayPauseController;
        }

        public void Attach(IPauseView view)
        {
            Detach();

            _view = view;
            _view.ResumeRequested += OnResumeRequested;
            _view.SaveRequested += OnSaveRequested;
            _view.LoadRequested += OnLoadRequested;
            _view.SettingsRequested += OnSettingsRequested;
            _view.ExitToMainMenuRequested += OnExitToMainMenuRequested;
        }

        public void Detach()
        {
            if (_view == null)
            {
                return;
            }

            _view.ResumeRequested -= OnResumeRequested;
            _view.SaveRequested -= OnSaveRequested;
            _view.LoadRequested -= OnLoadRequested;
            _view.SettingsRequested -= OnSettingsRequested;
            _view.ExitToMainMenuRequested -= OnExitToMainMenuRequested;
            _view = null;
        }

        private void OnResumeRequested() => _gameplayPauseController.Resume();
        private void OnSaveRequested() => _gameplayPauseController.SaveGame();
        private void OnLoadRequested() => _gameplayPauseController.LoadGame();
        private void OnSettingsRequested() => _gameplayPauseController.OpenSettings();
        private void OnExitToMainMenuRequested() => _gameplayPauseController.ExitToMainMenu();
    }
}
