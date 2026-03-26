using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public interface IGameStateMachine
    {
        UniTask Enter<TState>() where TState : class, IGameFlowState;
    }
}
