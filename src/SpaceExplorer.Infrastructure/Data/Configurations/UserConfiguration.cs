using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceExplorer.Domain.Entities;

namespace SpaceExplorer.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
