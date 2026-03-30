using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Infrastructure.Factories.Objects
{
    /// <summary>
    /// Контракт фабрики игровых объектов.
    /// </summary>
    public interface IGameObjectFactory
    {
        GameObject Instantiate(GameObject prefab = null, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null, DiContainer container = null);
        
        UniTask<T> InstantiateAndGetComponent<T>(GameObject prefab, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null, DiContainer container = null) where T : class =>
            (Instantiate(prefab, position, rotation, parent, container)).GetComponent<UniTask<T>>();

        void Destroy(GameObject gameObject);
        void Cleanup();
    }
}
