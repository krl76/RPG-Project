namespace prototype_Roma.Scripts
{
    /// <summary>
    /// Контракт состояния системы ввода.
    /// </summary>
    public interface IInputState
    {
        void EnterState();
        void ExitState();
    }
}
