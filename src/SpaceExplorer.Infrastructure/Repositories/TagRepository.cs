using Mapster;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Application.Features.Tags;
using SpaceExplorer.Application.Features.Tags.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Common;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Repositories;

public class TagRepository(AppDbContext context)
    : GenericRepository<Tag, TagDto, Guid>(context), ITagRepository
{
    public async Task<IEnumerable<TagDto>> GetByItemIdAsync(Guid collectionItemId, CancellationToken ct = default) =>
        await Context.CollectionItemTags
            .Where(ct2 => ct2.CollectionItemId == collectionItemId)
            .Select(ct2 => ct2.Tag)
            .ProjectToType<TagDto>()
            .ToListAsync(ct);

    public async Task AddToItemAsync(Guid collectionItemId, Guid tagId, CancellationToken ct = default)
    {
        var exists = await Context.CollectionItemTags
            .AnyAsync(ct2 => ct2.CollectionItemId == collectionItemId && ct2.TagId == tagId, ct);
        if (!exists)
        {
            Context.CollectionItemTags.Add(new CollectionItemTag
            {
                CollectionItemId = collectionItemId,
                TagId = tagId
            });
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveFromItemAsync(Guid collectionItemId, Guid tagId, CancellationToken ct = default)
    {
        var entry = await Context.CollectionItemTags
            .FirstOrDefaultAsync(ct2 => ct2.CollectionItemId == collectionItemId && ct2.TagId == tagId, ct);
        if (entry is not null)
        {
            Context.CollectionItemTags.Remove(entry);
            await Context.SaveChangesAsync(ct);
        }
    }
}
