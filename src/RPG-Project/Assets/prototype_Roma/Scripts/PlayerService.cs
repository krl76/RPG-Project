using UnityEngine;

namespace prototype_Roma.Scripts
{
    public class PlayerService : IPlayerService
    {
        public GameObject PlayerObject { get; private set; }
        public Transform PlayerTransform { get; private set; }
        
        public void InstallService()
        {
            PlayerObject = Object.FindAnyObjectByType<PlayerMovementBehaviour>().gameObject;
            PlayerTransform = PlayerObject.transform;
        }
    }
}