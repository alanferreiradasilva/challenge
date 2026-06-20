namespace SpaceExplorer.Application.Features.Images.Dtos;

public record CollectionItemDto(
    Guid Id,
    Guid CollectionId,
    string NasaImageId,
    string NasaImageUrl,
    string Title,
    string? Description,
    DateOnly? EarthDate,
    string? AiDescription,
    DateTime CreatedAt,
    IEnumerable<string> Tags);

public record AddItemRequest(
    string NasaImageId,
    string NasaImageUrl,
    string Title,
    string? Description,
    DateOnly? EarthDate);

public record EnrichItemRequest(string NasaImageId, string Title, string? Description);
public record EnrichItemResponse(string AiDescription, IEnumerable<string> Curiosities);

public record NasaImageDto(
    string NasaId,
    string Title,
    string? Description,
    string ImageUrl,
    string? Photographer,
    DateOnly? Date,
    string? Location);

public record NasaSearchRequest(
    string? Query,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? MediaType = "image",
    int Page = 1);
