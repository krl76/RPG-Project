using UnityEngine;
using Zenject;

namespace prototype_Roma.Scripts
{
    public class PlayerEventReader : MonoBehaviour
    {
        private IFightInputService _fightInputService;
        
        [Inject]
        private void Construct(IFightInputService fightInputService)
        {
            _fightInputService = fightInputService;
        }

        public void AttackAnimationCompleted()
        {
            _fightInputService.AttackEnd();
        }
    }
}