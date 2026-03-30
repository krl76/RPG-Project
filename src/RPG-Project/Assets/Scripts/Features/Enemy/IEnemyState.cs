namespace Features.Enemy
{
    /// <summary>
    /// Контракт состояния поведения врага.
    /// </summary>
    public interface IEnemyState
    {
        EnemyStateId Id { get; }
        void Enter();
        void Tick();
        void Exit();
    }
}
