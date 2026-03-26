using Features.Player;
using prototype_Roma.Scripts;
using UnityEngine;

namespace Infrastructure.Services.Player
{
    public class PlayerService : IPlayerService
    {
        public GameObject PlayerObject { get; private set; }
        public Transform PlayerTransform { get; private set; }
        
        public void InstallService()
        {
            PlayerObject = Object.FindAnyObjectByType<PlayerMovement>().gameObject;
            PlayerTransform = PlayerObject.transform;
        }
    }
}