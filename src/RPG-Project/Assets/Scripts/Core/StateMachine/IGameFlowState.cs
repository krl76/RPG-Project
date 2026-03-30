using Cysharp.Threading.Tasks;

namespace Core.StateMachine
{
    /// <summary>
    /// Контракт состояния игрового потока с асинхронным входом.
    /// </summary>
    public interface IGameFlowState : IExitableState
    {
        UniTask Enter();
    }
}
