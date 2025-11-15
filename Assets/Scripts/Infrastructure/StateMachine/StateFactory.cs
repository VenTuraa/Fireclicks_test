using Zenject;

namespace Fireclicks.Infrastructure.StateMachine
{
    public class StateFactory : IStateFactory
    {
        private readonly DiContainer _container;

        public StateFactory(DiContainer container)
        {
            _container = container;
        }

        public InitializationState CreateInitializationState()
        {
            return _container.Resolve<InitializationState>();
        }

        public LoadingState CreateLoadingState()
        {
            return _container.Resolve<LoadingState>();
        }

        public GameLoopState CreateGameLoopState()
        {
            return _container.Resolve<GameLoopState>();
        }
    }
}

