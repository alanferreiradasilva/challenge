using Microsoft.AspNetCore.Mvc;
using SpaceExplorer.Application.Features.Auth;
using SpaceExplorer.Application.Features.Auth.Dtos;

namespace SpaceExplorer.API.Endpoints;

public class AuthEndpoints : BaseEndpoints
{
    public override Task Map(WebApplication app)
    {
        app.MapPost("/api/auth/register", async ([FromServices] IAuthService svc, RegisterRequest request, CancellationToken ct) =>
        {
            var result = await svc.RegisterAsync(request, ct);
            return Results.Ok(result);
        }).WithName("Register");

        app.MapPost("/api/auth/login", async ([FromServices] IAuthService svc, LoginRequest request, CancellationToken ct) =>
        {
            var result = await svc.LoginAsync(request, ct);
            return Results.Ok(result);
        }).WithName("Login");

        return Task.CompletedTask;
    }
}
