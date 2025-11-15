using Cysharp.Threading.Tasks;

namespace Fireclicks.Infrastructure.StateMachine
{
    public interface IGameState
    {
        UniTask Enter();
        UniTask Exit();
    }
}

