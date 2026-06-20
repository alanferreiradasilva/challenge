using SpaceExplorer.Application.Common.Interfaces;
using SpaceExplorer.Application.Features.Collections.Dtos;

namespace SpaceExplorer.Application.Features.Collections;

public interface ICollectionRepository : IGenericRepository<CollectionDto, Guid>
{
    Task<IEnumerable<CollectionDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
