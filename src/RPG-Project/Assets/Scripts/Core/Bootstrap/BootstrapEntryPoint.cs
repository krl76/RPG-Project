using Core.StateMachine;
using Core.StateMachine.States;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Bootstrap
{
    public sealed class BootstrapEntryPoint : MonoBehaviour
    {
        private IGameStateMachine _gameStateMachine;

        [Inject]
        private void Construct(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        private void Start()
        {
            _gameStateMachine.Enter<BootstrapState>().Forget();
        }
    }
}
