using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Logging;
using Zenject;

namespace Fireclicks.Infrastructure.StateMachine
{
    public class GameLoopState : IGameState
    {
        private readonly Logger _logger;

        [Inject]
        public GameLoopState(Logger logger)
        {
            _logger = logger;
        }

        public UniTask Enter()
        {
            _logger.Log("GameLoopState: Game loop started");
            return UniTask.CompletedTask;
        }

        public UniTask Exit()
        {
            _logger.Log("GameLoopState: Exiting");
            return UniTask.CompletedTask;
        }
    }
}

