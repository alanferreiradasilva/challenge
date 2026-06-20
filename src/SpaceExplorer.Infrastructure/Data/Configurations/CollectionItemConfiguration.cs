using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceExplorer.Domain.Entities;

namespace SpaceExplorer.Infrastructure.Data.Configurations;

public class CollectionItemConfiguration : IEntityTypeConfiguration<CollectionItem>
{
    public void Configure(EntityTypeBuilder<CollectionItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.NasaImageId).IsRequired().HasMaxLength(200);
        builder.Property(i => i.NasaImageUrl).IsRequired().HasMaxLength(1000);
        builder.Property(i => i.Title).IsRequired().HasMaxLength(300);
        builder.HasOne(i => i.Collection)
               .WithMany(c => c.Items)
               .HasForeignKey(i => i.CollectionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
