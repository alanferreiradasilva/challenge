namespace SpaceExplorer.Domain.Entities;

public class CollectionItem : BaseEntity
{
    public Guid CollectionId { get; set; }
    public string NasaImageId { get; set; } = string.Empty;
    public string NasaImageUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? EarthDate { get; set; }
    public string? AiDescription { get; set; }

    public Collection Collection { get; set; } = null!;
    public ICollection<CollectionItemTag> CollectionItemTags { get; set; } = [];
}
