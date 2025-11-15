using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure;
using Fireclicks.Infrastructure.Logging;
using Fireclicks.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Logger = Fireclicks.Infrastructure.Logging.Logger;

namespace Fireclicks.Infrastructure.StateMachine
{
    public class LoadingState : IGameState, IDisposable
    {
        private readonly GameStateMachine _stateMachine;
        private readonly Logger _logger;
        private readonly LoadingScreenController _loadingScreenController;
        private readonly IStateFactory _stateFactory;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        [Inject]
        public LoadingState(
            GameStateMachine stateMachine,
            Logger logger,
            LoadingScreenController loadingScreenController,
            IStateFactory stateFactory)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loadingScreenController = loadingScreenController ?? throw new ArgumentNullException(nameof(loadingScreenController));
            _stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));
        }

        public async UniTask Enter()
        {
            if (_disposed)
            {
                _logger.LogError("LoadingState: Attempted to enter disposed state");
                return;
            }

            _logger.Log("LoadingState: Starting scene loading");

            _cancellationTokenSource = new CancellationTokenSource();

            _loadingScreenController.Show();

            string sceneAddress = GameConfig.GameLoopSceneAddress;

            _logger.Log($"Loading scene: {sceneAddress}");

            try
            {
                var progress = new Progress<float>(progressValue =>
                {
                    if (!_disposed && _loadingScreenController != null)
                    {
                        _loadingScreenController.SetProgress(progressValue * 100f);
                    }
                });

                var handle = Addressables.LoadSceneAsync(sceneAddress);
                await handle.ToUniTask(
                    progress: progress,
                    cancellationToken: _cancellationTokenSource.Token);

                if (_disposed)
                    return;

                _logger.Log("Scene loaded successfully");

                _loadingScreenController.Hide();

                var gameLoopState = _stateFactory.CreateGameLoopState();
                if (gameLoopState != null)
                {
                    await _stateMachine.ChangeState(gameLoopState);
                }
                else
                {
                    _logger.LogError("Failed to create GameLoopState");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("LoadingState: Loading was cancelled");
                _loadingScreenController.Hide();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load scene: {ex.Message}");
                _loadingScreenController.Hide();
                throw;
            }
        }

        public UniTask Exit()
        {
            _logger.Log("LoadingState: Exiting");

            CancelOperations();

            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            CancelOperations();
            _disposed = true;
        }

        private void CancelOperations()
        {
            if (_cancellationTokenSource != null)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error cancelling operations: {ex.Message}");
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }
    }
}
