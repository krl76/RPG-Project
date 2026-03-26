using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    public class SceneLoaderService : ISceneLoaderService
    {
        private readonly ILoadingScreenService _loadingScreenService;

        public SceneLoaderService(ILoadingScreenService loadingScreenService)
        {
            _loadingScreenService = loadingScreenService;
        }

        public async UniTask LoadSceneAsync(
            string sceneName,
            LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            _loadingScreenService.Show();
            _loadingScreenService.SetProgress(0f);

            await UniTask.NextFrame();

            var loadSceneAsync = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (loadSceneAsync == null)
            {
                _loadingScreenService.Hide();
                throw new InvalidOperationException($"Failed to load scene '{sceneName}'.");
            }

            try
            {
                while (loadSceneAsync.isDone == false)
                {
                    _loadingScreenService.SetProgress(Mathf.Clamp01(loadSceneAsync.progress / 0.9f));
                    await UniTask.Yield();
                }

                _loadingScreenService.SetProgress(1f);
                await UniTask.NextFrame();
            }
            finally
            {
                _loadingScreenService.Hide();
            }
        }
    }
}
