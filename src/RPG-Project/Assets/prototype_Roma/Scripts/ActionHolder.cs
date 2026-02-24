using System;
using UnityEngine.InputSystem;

namespace prototype_Roma.Scripts
{
    public class ActionHolder
    {
        public event Action Jump;
        public event Action SwordAttack;
        public event Action MagicAttack;
        public event Action<InputAction.CallbackContext> Sprint;
        public event Action<InputAction.CallbackContext> Move;

        public void JumpAction(InputAction.CallbackContext obj) => Jump?.Invoke();
        public void SwordAttackAction(InputAction.CallbackContext obj) => SwordAttack?.Invoke();
        public void MagicAttackAction(InputAction.CallbackContext obj) => MagicAttack?.Invoke();
        public void SprintAction(InputAction.CallbackContext obj) => Sprint?.Invoke(obj);
        public void MoveAction(InputAction.CallbackContext obj) => Move?.Invoke(obj);
    }
}