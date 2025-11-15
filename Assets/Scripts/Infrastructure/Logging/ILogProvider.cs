namespace Fireclicks.Infrastructure.Logging
{
    public interface ILogProvider
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}

