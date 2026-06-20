using SpaceExplorer.Application.Common.Interfaces;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.Features.Collections.Dtos;

namespace SpaceExplorer.Application.Features.Collections;

public interface ICollectionService : IGenericService<CollectionDto, CreateCollectionRequest, UpdateCollectionRequest, Guid>
{
    Task<IEnumerable<CollectionDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
