using Infrastructure.Services.Player.Input;
using UnityEngine;
using Zenject;

namespace MonoBehaviours.Player
{
    public class PlayerEventReader : MonoBehaviour
    {
        private IFightInputService _fightInputService;
        
        [Inject]
        private void Construct(IFightInputService fightInputService)
        {
            _fightInputService = fightInputService;
        }

        public void AttackCompleted()
        {
            _fightInputService.AttackEnd();
        }
    }
}