namespace SpaceExplorer.Application.Features.Tags.Dtos;

public record TagDto(Guid Id, string Name, Guid UserId);
public record CreateTagRequest(string Name);
public record TagSuggestionResponse(IEnumerable<string> Suggestions);
