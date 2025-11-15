using System.Collections.Concurrent;

namespace FireclicksServer.Services;

public sealed class RequestCounterService
{
    private readonly ConcurrentDictionary<string, int> _counts = new();

    public int Increment(string token)
    {
        return _counts.AddOrUpdate(token, 1, static (_, current) => current + 1);
    }
}
