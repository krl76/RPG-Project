using Infrastructure.Services.Events;
using Infrastructure.Services.Scene;
using Infrastructure.Services.UI;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class GameOverWindow : WindowBase, IGameStateSubscriber
    {
        public override WindowID Id => WindowID.GameOver;
        public override bool IsPopup => true;

        [SerializeField] private Button _restartButton;
        private ISceneLoaderService _sceneLoader;

        [Inject]
        public void Construct(ISceneLoaderService sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public override void OnOpen(object payload = null)
        {
            base.OnOpen(payload);
            EventBus.Subscribe(this);
            
            _restartButton.onClick.AddListener(RestartGame);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public override void OnClose()
        {
            base.OnClose();
            EventBus.Unsubscribe(this);
            _restartButton.onClick.RemoveListener(RestartGame);
        }

        public void OnGameOver()
        {

        }

        private void RestartGame()
        {
            _sceneLoader.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, 
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}