using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Infrastructure.Factories.Objects
{
    public class GameObjectFactory : IGameObjectFactory
    {
        private readonly DiContainer _globalContainer;
        
        private readonly List<GameObject> _trackedObjects = new();

        public GameObjectFactory(DiContainer globalContainer)
        {
            _globalContainer = globalContainer;
        }
        
        public GameObject Instantiate(GameObject prefab = null, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null, DiContainer container = null)
        {
            var containerToUse = container ?? _globalContainer;
            var obj = containerToUse.InstantiatePrefab(prefab, position ?? Vector3.zero,
                rotation ?? Quaternion.identity, parentTransform: parent);
            
            if (parent == null)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, 
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
            
            return Register(obj);
        }

        public T InstantiateAndGetComponent<T>(GameObject prefab, Vector3? position = null,
            Quaternion? rotation = null, Transform parent = null, DiContainer container = null) where T : class =>
            (Instantiate(prefab, position, rotation, parent, container)).GetComponent<T>();

        public void Destroy(GameObject gameObject)
        {
            if (gameObject != null)
            {
                _trackedObjects.Remove(gameObject);
                Object.Destroy(gameObject);
            }
        }

        public void Cleanup()
        {
            foreach (var obj in _trackedObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            _trackedObjects.Clear();
        }

        private GameObject Register(GameObject obj)
        {
            if (obj != null) _trackedObjects.Add(obj);
            return obj;
        }
    }
}