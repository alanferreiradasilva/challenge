using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpaceExplorer.Application.Features.Tags;
using SpaceExplorer.Application.Features.Tags.Dtos;

namespace SpaceExplorer.API.Endpoints;

public class TagEndpoints : BaseEndpoints
{
    public override Task Map(WebApplication app)
    {
        app.MapGet("/api/items/{itemId:guid}/tags", async ([FromServices] ITagService svc, Guid itemId, CancellationToken ct) =>
        {
            var result = await svc.GetByItemIdAsync(itemId, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("GetItemTags");

        app.MapPost("/api/items/{itemId:guid}/tags", async (ClaimsPrincipal user, [FromServices] ITagService svc, Guid itemId, CreateTagRequest request, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var result = await svc.CreateAndAddToItemAsync(itemId, request, userId, ct);
            return Results.Created($"/api/items/{itemId}/tags/{result.Id}", result);
        }).RequireAuthorization().WithName("AddItemTag");

        app.MapDelete("/api/items/{itemId:guid}/tags/{tagId:guid}", async ([FromServices] ITagService svc, Guid itemId, Guid tagId, CancellationToken ct) =>
        {
            await svc.RemoveFromItemAsync(itemId, tagId, ct);
            return Results.NoContent();
        }).RequireAuthorization().WithName("RemoveItemTag");

        app.MapGet("/api/items/{itemId:guid}/tags/suggestions", async ([FromServices] ITagService svc, Guid itemId, CancellationToken ct) =>
        {
            var result = await svc.SuggestTagsAsync(itemId, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("SuggestItemTags");

        return Task.CompletedTask;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
    }
}
