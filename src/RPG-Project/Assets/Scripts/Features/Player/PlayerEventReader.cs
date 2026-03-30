using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace Features.Player
{
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Принимает animation events игрока и передаёт их в animator service.
    /// </summary>
    public class PlayerEventReader : MonoBehaviour
    {
        private IPlayerAnimatorService _playerAnimatorService;

        [Inject]
        private void Construct(IPlayerAnimatorService playerAnimatorService)
        {
            _playerAnimatorService = playerAnimatorService;
        }

        public void OnAnimationEvent(string eventId)
        {
            _playerAnimatorService.ProcessAnimationEvent(eventId);
        }
    }
}
