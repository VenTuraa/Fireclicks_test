using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Logging;
using Fireclicks.Infrastructure.StateMachine;
using UnityEngine;
using Zenject;
using Logger = Fireclicks.Infrastructure.Logging.Logger;

namespace Fireclicks.Infrastructure
{
    public class Bootstrapper : MonoBehaviour
    {
        private GameStateMachine _stateMachine;
        private Logger _logger;
        private IStateFactory _stateFactory;
        private bool _initialized;

        [Inject]
        private void Construct(
            GameStateMachine stateMachine,
            Logger logger,
            IStateFactory stateFactory)
        {
            _stateMachine = stateMachine ?? throw new System.ArgumentNullException(nameof(stateMachine));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _stateFactory = stateFactory ?? throw new System.ArgumentNullException(nameof(stateFactory));
        }

        private void Start()
        {
            if (_initialized)
                return;

            if (!ValidateDependencies())
                return;

            StartAsync().Forget();
        }

        private bool ValidateDependencies()
        {
            if (_stateMachine == null)
            {
                Debug.LogError("[Bootstrapper] StateMachine is null!", this);
                return false;
            }

            if (_logger == null)
            {
                Debug.LogError("[Bootstrapper] Logger is null!", this);
                return false;
            }

            if (_stateFactory == null)
            {
                Debug.LogError("[Bootstrapper] StateFactory is null!", this);
                return false;
            }

            return true;
        }

        private async UniTaskVoid StartAsync()
        {
            try
            {
                var initialState = _stateFactory.CreateInitializationState();
                if (initialState == null)
                {
                    _logger.LogError("Failed to create InitializationState");
                    return;
                }

                await _stateMachine.ChangeState(initialState);
                _initialized = true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to start game: {ex.Message}");
                Debug.LogException(ex);
            }
        }
    }
}
