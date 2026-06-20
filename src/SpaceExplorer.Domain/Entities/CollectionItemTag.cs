namespace SpaceExplorer.Domain.Entities;

/// <summary>Join table: CollectionItem ↔ Tag (M:N)</summary>
public class CollectionItemTag
{
    public Guid CollectionItemId { get; set; }
    public Guid TagId { get; set; }

    public CollectionItem CollectionItem { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
