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
        private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> _loadingHandle = default;
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
                _loadingHandle = Addressables.LoadSceneAsync(sceneAddress);
                
                float artificialProgress = 0f;
                float lastRealProgress = 0f;
                float startTime = Time.realtimeSinceStartup;
                const float MIN_LOADING_TIME = 0.5f;
                const float MAX_LOADING_TIME = 3f;

                while (!_loadingHandle.IsDone)
                {
                    if (_disposed || _cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (_loadingHandle.IsValid())
                        {
                            Addressables.Release(_loadingHandle);
                        }
                        return;
                    }

                    float realProgress = GetRealProgress();
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    
                    if (realProgress > 0f && realProgress < 100f)
                    {
                        lastRealProgress = realProgress;
                        artificialProgress = realProgress;
                    }
                    else
                    {
                        if (elapsedTime < MIN_LOADING_TIME)
                        {
                            artificialProgress = Mathf.Clamp01(elapsedTime / MIN_LOADING_TIME) * 90f;
                        }
                        else if (elapsedTime < MAX_LOADING_TIME)
                        {
                            float remainingTime = MAX_LOADING_TIME - MIN_LOADING_TIME;
                            float progressInRemaining = Mathf.Clamp01((elapsedTime - MIN_LOADING_TIME) / remainingTime);
                            artificialProgress = 90f + (progressInRemaining * 10f);
                        }
                        else
                        {
                            artificialProgress = 99f;
                        }
                    }

                    artificialProgress = Mathf.Clamp(artificialProgress, 0f, 100f);

                    if (_loadingScreenController != null)
                    {
                        _loadingScreenController.SetProgress(artificialProgress);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
                }

                if (_disposed)
                {
                    if (_loadingHandle.IsValid())
                    {
                        Addressables.Release(_loadingHandle);
                    }
                    return;
                }

                if (_loadingHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    _logger.Log("Scene loaded successfully");
                    if (_loadingScreenController != null)
                    {
                        _loadingScreenController.SetProgressImmediate(100f);
                        await UniTask.Delay(200, cancellationToken: _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    _logger.LogError($"Scene loading failed: {_loadingHandle.OperationException?.Message ?? "Unknown error"}");
                    if (_loadingHandle.IsValid())
                    {
                        Addressables.Release(_loadingHandle);
                    }
                    throw new InvalidOperationException("Failed to load scene");
                }

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
                if (_loadingHandle.IsValid())
                {
                    Addressables.Release(_loadingHandle);
                }
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

        private float GetRealProgress()
        {
            if (!_loadingHandle.IsValid())
                return 0f;

            float progress = 0f;
            
            var downloadStatus = _loadingHandle.GetDownloadStatus();
            if (downloadStatus.TotalBytes > 0)
            {
                progress = downloadStatus.Percent * 100f;
            }
            
            if (progress <= 0f || progress > 100f || float.IsNaN(progress))
            {
                float percentComplete = _loadingHandle.PercentComplete;
                if (!float.IsNaN(percentComplete) && percentComplete >= 0f && percentComplete <= 1f)
                {
                    progress = percentComplete * 100f;
                }
            }

            return Mathf.Clamp(progress, 0f, 100f);
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

            if (_loadingHandle.IsValid())
            {
                try
                {
                    Addressables.Release(_loadingHandle);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error releasing loading handle: {ex.Message}");
                }
            }
        }
    }
}
