namespace SpaceExplorer.Application.Features.Collections.Dtos;

public record CollectionDto(Guid Id, string Name, string? Description, Guid UserId, DateTime CreatedAt, int ItemCount);
public record CreateCollectionRequest(string Name, string? Description);
public record UpdateCollectionRequest(string Name, string? Description);
