using SpaceExplorer.Application.Common.Interfaces;
using SpaceExplorer.Application.Features.Tags.Dtos;

namespace SpaceExplorer.Application.Features.Tags;

public interface ITagRepository : IGenericRepository<TagDto, Guid>
{
    Task<IEnumerable<TagDto>> GetByItemIdAsync(Guid collectionItemId, CancellationToken ct = default);
    Task AddToItemAsync(Guid collectionItemId, Guid tagId, CancellationToken ct = default);
    Task RemoveFromItemAsync(Guid collectionItemId, Guid tagId, CancellationToken ct = default);
}
