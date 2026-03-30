using Features.Player;
using UnityEngine;

namespace Infrastructure.Services.Player
{
    /// <summary>
    /// Хранит ссылки на активного игрока в сцене.
    /// </summary>
    public class PlayerService : IPlayerService
    {
        public GameObject PlayerObject { get; private set; }
        public Transform PlayerTransform { get; private set; }

        public void InstallService()
        {
            var playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("[PlayerService] PlayerMovement was not found in the active scene.");
                PlayerObject = null;
                PlayerTransform = null;
                return;
            }

            PlayerObject = playerMovement.gameObject;
            PlayerTransform = PlayerObject.transform;
        }
    }
}
