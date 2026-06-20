using Mapster;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Application.Features.Images;
using SpaceExplorer.Application.Features.Images.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Common;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Repositories;

public class CollectionItemRepository(AppDbContext context)
    : GenericRepository<CollectionItem, CollectionItemDto, Guid>(context), ICollectionItemRepository
{
    public async Task<IEnumerable<CollectionItemDto>> GetByCollectionIdAsync(Guid collectionId, CancellationToken ct = default) =>
        await DbSet
            .Where(i => i.CollectionId == collectionId)
            .Include(i => i.CollectionItemTags).ThenInclude(ct2 => ct2.Tag)
            .ProjectToType<CollectionItemDto>()
            .ToListAsync(ct);

    public async Task<IEnumerable<CollectionItemDto>> GetByUserIdOrderedByDateAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet
            .Where(i => i.Collection.UserId == userId && i.EarthDate != null)
            .Include(i => i.CollectionItemTags).ThenInclude(ct2 => ct2.Tag)
            .OrderBy(i => i.EarthDate)
            .ProjectToType<CollectionItemDto>()
            .ToListAsync(ct);
}
