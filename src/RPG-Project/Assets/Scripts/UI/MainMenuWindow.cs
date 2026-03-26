using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.UI;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public sealed class MainMenuWindow : WindowBase
    {
        public override WindowID Id => WindowID.MainMenu;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private IGameStateMachine _gameStateMachine;
        private IWindowService _windowService;

        [Inject]
        private void Construct(IGameStateMachine gameStateMachine, IWindowService windowService)
        {
            _gameStateMachine = gameStateMachine;
            _windowService = windowService;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            _startButton.onClick.AddListener(StartGame);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(ExitGame);
        }

        public override void OnClose()
        {
            _startButton.onClick.RemoveListener(StartGame);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(ExitGame);

            base.OnClose();
        }

        private void StartGame()
        {
            StartGameAsync().Forget();
        }

        private async UniTask StartGameAsync()
        {
            await _gameStateMachine.Enter<LoadGameState>();
        }

        private void OpenSettings()
        {
            if (_windowService.IsWindowOpened(WindowID.Settings))
            {
                return;
            }

            _windowService.Open(WindowID.Settings);
        }

        private static void ExitGame()
        {
            Application.Quit();
        }
    }
}
