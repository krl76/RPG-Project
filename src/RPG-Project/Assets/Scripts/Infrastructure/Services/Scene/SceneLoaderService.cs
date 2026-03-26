using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public async UniTask LoadSceneAsync(
            string sceneName,
            LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            var loadSceneAsync = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (loadSceneAsync == null)
            {
                throw new InvalidOperationException($"Failed to load scene '{sceneName}'.");
            }

            await loadSceneAsync.ToUniTask();
        }
    }
}
