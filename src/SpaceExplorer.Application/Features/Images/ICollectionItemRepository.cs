using SpaceExplorer.Application.Common.Interfaces;
using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Application.Features.Images;

public interface ICollectionItemRepository : IGenericRepository<CollectionItemDto, Guid>
{
    Task<IEnumerable<CollectionItemDto>> GetByCollectionIdAsync(Guid collectionId, CancellationToken ct = default);
    Task<IEnumerable<CollectionItemDto>> GetByUserIdOrderedByDateAsync(Guid userId, CancellationToken ct = default);
}
