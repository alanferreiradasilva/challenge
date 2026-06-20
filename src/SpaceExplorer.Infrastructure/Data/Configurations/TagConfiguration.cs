using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceExplorer.Domain.Entities;

namespace SpaceExplorer.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.HasOne(t => t.User)
               .WithMany(u => u.Tags)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(t => new { t.UserId, t.Name }).IsUnique();
    }
}

public class CollectionItemTagConfiguration : IEntityTypeConfiguration<CollectionItemTag>
{
    public void Configure(EntityTypeBuilder<CollectionItemTag> builder)
    {
        builder.HasKey(ct => new { ct.CollectionItemId, ct.TagId });
        builder.HasOne(ct => ct.CollectionItem)
               .WithMany(i => i.CollectionItemTags)
               .HasForeignKey(ct => ct.CollectionItemId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ct => ct.Tag)
               .WithMany(t => t.CollectionItemTags)
               .HasForeignKey(ct => ct.TagId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
