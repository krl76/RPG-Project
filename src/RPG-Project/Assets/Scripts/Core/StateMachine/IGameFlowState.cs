using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    public interface IGameFlowState : IExitableState
    {
        UniTask Enter();
    }
}
