using System.Collections.Generic;

namespace Features.Enemy
{
    /// <summary>
    /// Простая машина состояний для поведения врага.
    /// </summary>
    public sealed class EnemyStateMachine
    {
        private readonly Dictionary<EnemyStateId, IEnemyState> _states = new Dictionary<EnemyStateId, IEnemyState>();

        private IEnemyState _currentState;

        public EnemyStateId CurrentStateId => _currentState?.Id ?? EnemyStateId.None;

        public void AddState(IEnemyState state)
        {
            if (state == null)
            {
                return;
            }

            _states[state.Id] = state;
        }

        public void Enter(EnemyStateId stateId)
        {
            if (_states.TryGetValue(stateId, out IEnemyState nextState) == false)
            {
                return;
            }

            if (_currentState == nextState)
            {
                return;
            }

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        public void Tick()
        {
            _currentState?.Tick();
        }
    }
}
