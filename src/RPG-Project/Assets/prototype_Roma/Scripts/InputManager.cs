namespace prototype_Roma.Scripts
{
    public class InputManager
    {
        public readonly ActionHolder Actions;
        
        private IInputState _currentState;
        
        public readonly GameplayInputState GameplayInputState;
        
        public InputManager(PlayerInput playerInput)
        {
            Actions = new ActionHolder();

            GameplayInputState = new GameplayInputState(playerInput, Actions);
        }
        
        public void ChangeState(IInputState newState)
        {
            _currentState?.ExitState();
            _currentState = newState;
            _currentState.EnterState();
        }
    }
}