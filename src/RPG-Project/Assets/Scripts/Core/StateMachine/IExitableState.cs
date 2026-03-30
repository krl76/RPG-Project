namespace Core.StateMachine
{
    /// <summary>
    /// Контракт состояния, поддерживающего выход.
    /// </summary>
    public interface IExitableState
    {
        void Exit();
    }
}
