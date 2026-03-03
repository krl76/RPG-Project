using Infrastructure.Services.Player.Animator;
using UnityEngine;
using Zenject;

namespace MonoBehaviours.Player
{
    [RequireComponent(typeof(Animator))]
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