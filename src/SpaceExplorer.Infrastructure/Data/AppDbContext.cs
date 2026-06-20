using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Domain.Entities;

namespace SpaceExplorer.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<CollectionItemTag> CollectionItemTags => Set<CollectionItemTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
