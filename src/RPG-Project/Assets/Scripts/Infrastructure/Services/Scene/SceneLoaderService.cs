using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public async UniTask LoadSceneAsync(string sceneName, 
            LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            var loadSceneAsync = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        }
    }
}