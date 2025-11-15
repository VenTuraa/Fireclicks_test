using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Logging;
using Fireclicks.Infrastructure.Services;
using Fireclicks.UI;
using Zenject;

namespace Fireclicks.Infrastructure.StateMachine
{
    public class InitializationState : IGameState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly Logger _logger;
        private readonly CpuFrameTimeOverlay _cpuFrameTimeOverlay;
        private readonly IStateFactory _stateFactory;
        private readonly EncryptedTokenStorage _tokenStorage;

        [Inject]
        public InitializationState(
            GameStateMachine stateMachine,
            Logger logger,
            CpuFrameTimeOverlay cpuFrameTimeOverlay,
            IStateFactory stateFactory,
            EncryptedTokenStorage tokenStorage)
        {
            _stateMachine = stateMachine;
            _logger = logger;
            _cpuFrameTimeOverlay = cpuFrameTimeOverlay;
            _stateFactory = stateFactory;
            _tokenStorage = tokenStorage;
        }

        public async UniTask Enter()
        {
            _logger.Log("InitializationState: Starting initialization");

            await _tokenStorage.InitializeAsync();
            _logger.Log("User token initialized");

            _logger.Log("Logger initialized");

            _cpuFrameTimeOverlay.Initialize();
            _logger.Log("CPU Frame Time overlay initialized");

            await UniTask.DelayFrame(1);

            await _stateMachine.ChangeState(_stateFactory.CreateLoadingState());
        }

        public UniTask Exit()
        {
            _logger.Log("InitializationState: Exiting");
            return UniTask.CompletedTask;
        }
    }
}

