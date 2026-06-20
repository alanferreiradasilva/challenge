namespace SpaceExplorer.Domain.Entities;

public class Collection : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<CollectionItem> Items { get; set; } = [];
}
