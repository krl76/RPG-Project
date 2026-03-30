using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    /// <summary>
    /// Контракт машины состояний игры.
    /// </summary>
    public interface IGameStateMachine
    {
        UniTask Enter<TState>() where TState : class, IGameFlowState;
    }
}
