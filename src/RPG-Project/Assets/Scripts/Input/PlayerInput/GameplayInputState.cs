namespace prototype_Roma.Scripts
{
    public class GameplayInputState : IInputState
    {
        private readonly PlayerInput _input;
        private readonly ActionHolder _actionHolder;

        public GameplayInputState(PlayerInput playerInput, ActionHolder holder)
        {
            _input = playerInput;
            _actionHolder = holder;
        }
        
        public void EnterState()
        {
            _input.Player.Enable();
            
            _input.Player.Jump.performed += _actionHolder.JumpAction;

            _input.Player.Move.started += _actionHolder.MoveAction;
            _input.Player.Move.performed += _actionHolder.MoveAction;
            _input.Player.Move.canceled += _actionHolder.MoveAction;

            _input.Player.Sprint.started += _actionHolder.SprintAction;
            _input.Player.Sprint.canceled += _actionHolder.SprintAction;

            _input.Player.SwordAttack.performed += _actionHolder.SwordAttackAction;

            _input.Player.MagicAttack.performed += _actionHolder.MagicAttackAction;
            
        }

        public void ExitState()
        {
            _input.Player.Disable();
            
            _input.Player.Jump.performed -= _actionHolder.JumpAction;

            _input.Player.Move.started -= _actionHolder.MoveAction;
            _input.Player.Move.performed -= _actionHolder.MoveAction;
            _input.Player.Move.canceled -= _actionHolder.MoveAction;

            _input.Player.Sprint.started -= _actionHolder.SprintAction;
            _input.Player.Sprint.canceled -= _actionHolder.SprintAction;

            _input.Player.SwordAttack.performed -= _actionHolder.SwordAttackAction;

            _input.Player.MagicAttack.performed -= _actionHolder.MagicAttackAction;
        }
    }
}