using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.Features.Collections;
using SpaceExplorer.Application.Features.Collections.Dtos;

namespace SpaceExplorer.API.Endpoints;

public class CollectionEndpoints : BaseEndpoints
{
    public override Task Map(WebApplication app)
    {
        app.MapGet("/api/collections", async (ClaimsPrincipal user, [FromServices] ICollectionService svc, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var result = await svc.GetByUserIdAsync(userId, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("GetCollections");

        app.MapPost("/api/collections", async (ClaimsPrincipal user, [FromServices] ICollectionService svc, CreateCollectionRequest request, CancellationToken ct) =>
        {
            var result = await svc.CreateAsync(request, ct);
            return Results.Created($"/api/collections/{result.Id}", result);
        }).RequireAuthorization().WithName("CreateCollection");

        app.MapGet("/api/collections/{id:guid}", async ([FromServices] ICollectionService svc, Guid id, CancellationToken ct) =>
        {
            var result = await svc.GetByIdAsync(id, ct);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        }).RequireAuthorization().WithName("GetCollectionById");

        app.MapPut("/api/collections/{id:guid}", async ([FromServices] ICollectionService svc, Guid id, UpdateCollectionRequest request, CancellationToken ct) =>
        {
            var result = await svc.UpdateAsync(id, request, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("UpdateCollection");

        app.MapDelete("/api/collections/{id:guid}", async ([FromServices] ICollectionService svc, Guid id, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization().WithName("DeleteCollection");

        return Task.CompletedTask;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
    }
}
