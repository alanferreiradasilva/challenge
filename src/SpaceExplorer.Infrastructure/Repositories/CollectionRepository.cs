using Mapster;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Application.Features.Collections;
using SpaceExplorer.Application.Features.Collections.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Common;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Repositories;

public class CollectionRepository(AppDbContext context)
    : GenericRepository<Collection, CollectionDto, Guid>(context), ICollectionRepository
{
    public async Task<IEnumerable<CollectionDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.Items)
            .ProjectToType<CollectionDto>()
            .ToListAsync(ct);
}
