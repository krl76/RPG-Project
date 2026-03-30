using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    /// <summary>
    /// Контракт асинхронной загрузки сцен.
    /// </summary>
    public interface ISceneLoaderService
    {
        UniTask LoadSceneAsync(string sceneName,
            LoadSceneMode loadSceneMode = LoadSceneMode.Additive);
    }
}
