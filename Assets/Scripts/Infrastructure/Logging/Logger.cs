using System.Diagnostics;

namespace Fireclicks.Infrastructure.Logging
{
    public class Logger
    {
        private readonly ILogProvider _logProvider;

        public Logger(ILogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new System.ArgumentNullException(nameof(logProvider));
        }

        [Conditional("ENABLE_LOGS")]
        public void Log(string message)
        {
            _logProvider.Log(message);
        }

        [Conditional("ENABLE_LOGS")]
        public void LogWarning(string message)
        {
            _logProvider.LogWarning(message);
        }

        [Conditional("ENABLE_LOGS")]
        public void LogError(string message)
        {
            _logProvider.LogError(message);
        }
    }
}
