using Mapster;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.Features.Collections;
using SpaceExplorer.Application.Features.Collections.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Repositories;

public class CollectionService(ICollectionRepository repository) : ICollectionService
{
    public Task<CollectionDto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, ct);

    public Task<PagedResult<CollectionDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default) =>
        repository.GetAllAsync(request, ct);

    public async Task<CollectionDto> CreateAsync(CreateCollectionRequest request, CancellationToken ct = default)
    {
        var dto = new CollectionDto(Guid.Empty, request.Name, request.Description, Guid.Empty, DateTime.UtcNow, 0);
        return await repository.CreateAsync(dto, ct);
    }

    public async Task<CollectionDto> UpdateAsync(Guid id, UpdateCollectionRequest request, CancellationToken ct = default)
    {
        var existing = await repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Collection {id} not found.");
        var updated = existing with { Name = request.Name, Description = request.Description };
        return await repository.UpdateAsync(updated, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default) =>
        repository.DeleteAsync(id, ct);

    public Task<IEnumerable<CollectionDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        repository.GetByUserIdAsync(userId, ct);
}
