namespace Features.Enemy.States
{
    /// <summary>
    /// Базовый класс состояний врага с доступом к `EnemyAI` и state machine.
    /// </summary>
    public abstract class EnemyStateBase : IEnemyState
    {
        protected readonly EnemyAI Enemy;
        protected readonly EnemyStateMachine StateMachine;

        protected EnemyStateBase(EnemyAI enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public abstract EnemyStateId Id { get; }

        public abstract void Enter();

        public abstract void Tick();

        public virtual void Exit()
        {
        }
    }
}
