namespace Fireclicks.Infrastructure.StateMachine
{
    public interface IStateFactory
    {
        InitializationState CreateInitializationState();
        LoadingState CreateLoadingState();
        GameLoopState CreateGameLoopState();
    }
}

