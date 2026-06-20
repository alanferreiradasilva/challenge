using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Tags;
using SpaceExplorer.Application.Features.Tags.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Data;

namespace SpaceExplorer.Infrastructure.Repositories;

public class TagService(ITagRepository repository, IAiService aiService, AppDbContext context) : ITagService
{
    public Task<IEnumerable<TagDto>> GetByItemIdAsync(Guid collectionItemId, CancellationToken ct = default) =>
        repository.GetByItemIdAsync(collectionItemId, ct);

    public async Task<TagDto> CreateAndAddToItemAsync(Guid collectionItemId, CreateTagRequest request, Guid userId, CancellationToken ct = default)
    {
        var existing = await context.Tags
            .FirstOrDefaultAsync(t => t.Name == request.Name && t.UserId == userId, ct);

        Tag tag;
        if (existing is null)
        {
            tag = new Tag { Id = Guid.NewGuid(), Name = request.Name, UserId = userId, CreatedAt = DateTime.UtcNow };
            await context.Tags.AddAsync(tag, ct);
            await context.SaveChangesAsync(ct);
        }
        else
        {
            tag = existing;
        }

        await repository.AddToItemAsync(collectionItemId, tag.Id, ct);
        return new TagDto(tag.Id, tag.Name, tag.UserId);
    }

    public Task RemoveFromItemAsync(Guid collectionItemId, Guid tagId, CancellationToken ct = default) =>
        repository.RemoveFromItemAsync(collectionItemId, tagId, ct);

    public async Task<TagSuggestionResponse> SuggestTagsAsync(Guid collectionItemId, CancellationToken ct = default)
    {
        var item = await context.CollectionItems.FindAsync([collectionItemId], ct)
            ?? throw new KeyNotFoundException($"CollectionItem {collectionItemId} not found.");

        var suggestions = await aiService.SuggestTagsAsync(item.Title, item.Description, ct);
        return new TagSuggestionResponse(suggestions);
    }
}
