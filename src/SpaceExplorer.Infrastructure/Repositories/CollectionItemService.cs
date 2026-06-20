using Mapster;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Images;
using SpaceExplorer.Application.Features.Images.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Repositories;

public class CollectionItemService(
    ICollectionItemRepository repository,
    IAiService aiService) : ICollectionItemService
{
    public Task<IEnumerable<CollectionItemDto>> GetByCollectionIdAsync(Guid collectionId, CancellationToken ct = default) =>
        repository.GetByCollectionIdAsync(collectionId, ct);

    public async Task<CollectionItemDto> AddToCollectionAsync(Guid collectionId, AddItemRequest request, CancellationToken ct = default)
    {
        var dto = new CollectionItemDto(
            Guid.Empty, collectionId,
            request.NasaImageId, request.NasaImageUrl,
            request.Title, request.Description,
            request.EarthDate, null,
            DateTime.UtcNow, []);
        return await repository.CreateAsync(dto, ct);
    }

    public Task RemoveFromCollectionAsync(Guid collectionId, Guid itemId, CancellationToken ct = default) =>
        repository.DeleteAsync(itemId, ct);

    public async Task<CollectionItemDto> EnrichWithAiAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(itemId, ct)
            ?? throw new KeyNotFoundException($"CollectionItem {itemId} not found.");

        var enriched = await aiService.EnrichImageAsync(
            new EnrichItemRequest(item.NasaImageId, item.Title, item.Description), ct);

        var updated = item with { AiDescription = enriched.AiDescription };
        return await repository.UpdateAsync(updated, ct);
    }

    public Task<IEnumerable<CollectionItemDto>> GetTimelineByUserAsync(Guid userId, CancellationToken ct = default) =>
        repository.GetByUserIdOrderedByDateAsync(userId, ct);
}
