using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpaceExplorer.Infrastructure.Data;

/// <summary>
/// Allows EF Core tools (dotnet ef migrations add) to create AppDbContext at
/// design-time without needing the full application host. The connection string
/// used here is only for model discovery — it does not need to point to a real
/// database when just generating migration files.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=spaceexplorer;Username=spaceexplorer;Password=dev_password";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}
