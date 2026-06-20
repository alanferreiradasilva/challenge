using SpaceExplorer.Application.Features.Tags.Dtos;

namespace SpaceExplorer.Application.Features.Tags;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetByItemIdAsync(Guid collectionItemId, CancellationToken ct = default);
    Task<TagDto> CreateAndAddToItemAsync(Guid collectionItemId, CreateTagRequest request, Guid userId, CancellationToken ct = default);
    Task RemoveFromItemAsync(Guid collectionItemId, Guid tagId, CancellationToken ct = default);
    Task<TagSuggestionResponse> SuggestTagsAsync(Guid collectionItemId, CancellationToken ct = default);
}
