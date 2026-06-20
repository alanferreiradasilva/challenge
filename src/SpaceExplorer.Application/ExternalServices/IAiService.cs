using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Application.ExternalServices;

public interface IAiService
{
    Task<EnrichItemResponse> EnrichImageAsync(EnrichItemRequest request, CancellationToken ct = default);
    Task<IEnumerable<string>> SuggestTagsAsync(string title, string? description, CancellationToken ct = default);
}
