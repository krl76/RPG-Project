namespace Features.Enemy
{
    public interface IEnemyState
    {
        EnemyStateId Id { get; }
        void Enter();
        void Tick();
        void Exit();
    }
}
