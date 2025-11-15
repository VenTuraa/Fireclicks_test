namespace FireclicksServer.Models;

public sealed record RequestCountRequest
{
    public string Token { get; init; } = string.Empty;
}
