using FireclicksServer.Models;
using FireclicksServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RequestCounterService>();
builder.Services.AddSingleton<TokenCryptoService>();

var app = builder.Build();

app.MapGet("/", () => "Fireclicks Request Counter API");

app.MapPost("/api/request-count", (
    RequestCountRequest request,
    TokenCryptoService crypto,
    RequestCounterService counter) =>
{
    if (request is null || string.IsNullOrWhiteSpace(request.Token))
    {
        return Results.BadRequest(new { error = "TokenMissing" });
    }

    var decrypted = crypto.TryDecryptToken(request.Token);
    if (string.IsNullOrWhiteSpace(decrypted))
    {
        return Results.BadRequest(new { error = "TokenInvalid" });
    }

    int count = counter.Increment(decrypted);
    return Results.Ok(new RequestCountResponse(decrypted, count));
});

app.Run();
