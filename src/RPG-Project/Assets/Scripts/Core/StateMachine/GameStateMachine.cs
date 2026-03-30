using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.StateMachine
{
    /// <summary>
    /// Управляет переключением состояний игрового flow.
    /// </summary>
    public sealed class GameStateMachine : IGameStateMachine
    {
        private readonly DiContainer _container;

        private IExitableState _activeState;

        public GameStateMachine(DiContainer container)
        {
            _container = container;
        }

        public async UniTask Enter<TState>() where TState : class, IGameFlowState
        {
            _activeState?.Exit();

            TState state = _container.Resolve<TState>();
            _activeState = state;

            await state.Enter();
        }
    }
}
