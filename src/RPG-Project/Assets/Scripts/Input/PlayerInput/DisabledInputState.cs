using prototype_Roma.Scripts;

namespace Input.PlayerInput
{
    public sealed class DisabledInputState : IInputState
    {
        private readonly global::PlayerInput _input;

        public DisabledInputState(global::PlayerInput input)
        {
            _input = input;
        }

        public void EnterState()
        {
            _input.Player.Disable();
        }

        public void ExitState()
        {
        }
    }
}
