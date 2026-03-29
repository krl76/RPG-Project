using Data.Configs;

namespace Features.Enemy.States
{
    public sealed class EnemyRestState : EnemyStateBase
    {
        public EnemyRestState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Rest;

        public override void Enter()
        {
            Enemy.StopMovement();
        }

        public override void Tick()
        {
            if (Enemy.ShouldFlee())
            {
                StateMachine.Enter(EnemyStateId.Flee);
                return;
            }

            if (Enemy.Config.BehaviourType == EnemyBehaviourType.Boss)
            {
                if (Enemy.ShouldActivateBoss())
                {
                    StateMachine.Enter(EnemyStateId.Aggression);
                }

                return;
            }

            if (Enemy.ShouldActivateRegularEnemy())
            {
                StateMachine.Enter(EnemyStateId.Aggression);
            }
        }
    }
}
