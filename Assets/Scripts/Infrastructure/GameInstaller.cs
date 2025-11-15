using Fireclicks.Infrastructure.Logging;
using Fireclicks.Infrastructure.Networking;
using Fireclicks.Infrastructure.Services;
using Fireclicks.Infrastructure.StateMachine;
using Fireclicks.UI;
using UnityEngine;
using Zenject;
using Logger = Fireclicks.Infrastructure.Logging.Logger;

namespace Fireclicks.Infrastructure
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ILogProvider>().To<UnityLogProvider>().AsSingle();
            
            Container.Bind<Logger>().AsSingle();

            Container.Bind<GameStateMachine>().AsSingle();

            Container.Bind<EncryptedTokenStorage>().AsSingle();
            Container.Bind<RequestCounterApiService>().AsSingle().NonLazy();

            Container.Bind<IStateFactory>().To<StateFactory>().AsSingle();

            Container.Bind<InitializationState>().AsTransient();
            Container.Bind<LoadingState>().AsTransient();
            Container.Bind<GameLoopState>().AsTransient();

            BindLoadingScreenController();
            
            BindCpuFrameTimeOverlay();

            BindBootstrapper();
        }

        private void BindLoadingScreenController()
        {
            Container.Bind<LoadingScreenController>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("LoadingScreenController")
                .AsSingle()
                .NonLazy();
        }

        private void BindCpuFrameTimeOverlay()
        {
            Container.Bind<CpuFrameTimeOverlay>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("CpuFrameTimeOverlay")
                .AsSingle()
                .NonLazy();
        }

        private void BindBootstrapper()
        {
            Container.Bind<Bootstrapper>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}

