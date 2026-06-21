using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Images;
using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.API.Endpoints;

public class ImageEndpoints : BaseEndpoints
{
    public override Task Map(WebApplication app)
    {
        app.MapGet("/api/images/search", async ([FromServices] INasaService svc, [AsParameters] NasaSearchRequest request, CancellationToken ct) =>
        {
            var result = await svc.SearchAsync(request, ct);
            return Results.Ok(result);
        }).WithName("SearchNasaImages");

        app.MapGet("/api/collections/{collectionId:guid}/items", async ([FromServices] ICollectionItemService svc, Guid collectionId, CancellationToken ct) =>
        {
            var result = await svc.GetByCollectionIdAsync(collectionId, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("GetCollectionItems");

        app.MapPost("/api/collections/{collectionId:guid}/items", async ([FromServices] ICollectionItemService svc, Guid collectionId, AddItemRequest request, CancellationToken ct) =>
        {
            var result = await svc.AddToCollectionAsync(collectionId, request, ct);
            return Results.Created($"/api/collections/{collectionId}/items/{result.Id}", result);
        }).RequireAuthorization().WithName("AddCollectionItem");

        app.MapDelete("/api/collections/{collectionId:guid}/items/{itemId:guid}", async ([FromServices] ICollectionItemService svc, Guid collectionId, Guid itemId, CancellationToken ct) =>
        {
            await svc.RemoveFromCollectionAsync(collectionId, itemId, ct);
            return Results.NoContent();
        }).RequireAuthorization().WithName("RemoveCollectionItem");

        app.MapPost("/api/items/{itemId:guid}/enrich", async ([FromServices] ICollectionItemService svc, Guid itemId, CancellationToken ct) =>
        {
            var result = await svc.EnrichWithAiAsync(itemId, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("EnrichItem");

        app.MapGet("/api/timeline", async (ClaimsPrincipal user, [FromServices] ICollectionItemService svc, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var result = await svc.GetTimelineByUserAsync(userId, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("GetTimeline");

        return Task.CompletedTask;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
    }
}
