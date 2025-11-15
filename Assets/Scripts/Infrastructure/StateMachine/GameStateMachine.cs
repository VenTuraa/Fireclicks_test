using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Logging;
using Zenject;

namespace Fireclicks.Infrastructure.StateMachine
{
    public class GameStateMachine
    {
        private IGameState _currentState;
        private readonly Logger _logger;

        [Inject]
        public GameStateMachine(Logger logger)
        {
            _logger = logger;
        }

        public async UniTask ChangeState(IGameState newState)
        {
            if (newState == null)
            {
                _logger.LogError("Attempted to change to null state");
                return;
            }

            if (_currentState != null)
            {
                try
                {
                    _logger.Log($"Exiting state: {_currentState.GetType().Name}");
                    await _currentState.Exit();
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error exiting state {_currentState.GetType().Name}: {ex.Message}");
                    UnityEngine.Debug.LogException(ex);
                }
            }

            _currentState = newState;
            try
            {
                _logger.Log($"Entering state: {_currentState.GetType().Name}");
                await _currentState.Enter();
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error entering state {_currentState.GetType().Name}: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
                _currentState = null;
                throw;
            }
        }
    }
}

