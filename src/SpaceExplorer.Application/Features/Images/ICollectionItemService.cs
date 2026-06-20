using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Application.Features.Images;

public interface ICollectionItemService
{
    Task<IEnumerable<CollectionItemDto>> GetByCollectionIdAsync(Guid collectionId, CancellationToken ct = default);
    Task<CollectionItemDto> AddToCollectionAsync(Guid collectionId, AddItemRequest request, CancellationToken ct = default);
    Task RemoveFromCollectionAsync(Guid collectionId, Guid itemId, CancellationToken ct = default);
    Task<CollectionItemDto> EnrichWithAiAsync(Guid itemId, CancellationToken ct = default);
    Task<IEnumerable<CollectionItemDto>> GetTimelineByUserAsync(Guid userId, CancellationToken ct = default);
}
