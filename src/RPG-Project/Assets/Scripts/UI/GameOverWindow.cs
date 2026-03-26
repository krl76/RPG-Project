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
    public class GameOverWindow : WindowBase
    {
        public override WindowID Id => WindowID.GameOver;
        public override bool IsPopup => true;

        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _backToMenuButton;

        private IGameStateMachine _gameStateMachine;

        [Inject]
        private void Construct(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);

            _restartButton.onClick.AddListener(RestartGame);
            _backToMenuButton.onClick.AddListener(BackToMenu);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            _restartButton.onClick.RemoveListener(RestartGame);
            _backToMenuButton.onClick.RemoveListener(BackToMenu);
        }

        private void RestartGame()
        {
            RestartGameAsync().Forget();
        }

        private void BackToMenu()
        {
            BackToMenuAsync().Forget();
        }

        private async UniTask RestartGameAsync()
        {
            await _gameStateMachine.Enter<LoadGameState>();
        }

        private async UniTask BackToMenuAsync()
        {
            await _gameStateMachine.Enter<LoadMainMenuState>();
        }
    }
}
