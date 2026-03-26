using prototype_Roma.Scripts;

namespace Input.PlayerInput
{
    public class InputManager
    {
        public readonly ActionHolder Actions;
        
        private IInputState _currentState;
        
        public readonly GameplayInputState GameplayInputState;
        public readonly DisabledInputState DisabledInputState;
        
        public InputManager(global::PlayerInput playerInput)
        {
            Actions = new ActionHolder();

            GameplayInputState = new GameplayInputState(playerInput, Actions);
            DisabledInputState = new DisabledInputState(playerInput);
        }
        
        public void ChangeState(IInputState newState)
        {
            _currentState?.ExitState();
            _currentState = newState;
            _currentState.EnterState();
        }
    }
}
