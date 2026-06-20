using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Auth;
using SpaceExplorer.Application.Features.Collections;
using SpaceExplorer.Application.Features.Images;
using SpaceExplorer.Application.Features.Tags;
using SpaceExplorer.Infrastructure.Data;
using SpaceExplorer.Infrastructure.ExternalServices;
using SpaceExplorer.Infrastructure.Repositories;
using System.Text;

namespace SpaceExplorer.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"))
                   .UseSnakeCaseNamingConvention());

        // Auth
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<ICollectionItemRepository, CollectionItemRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<ICollectionItemService, CollectionItemService>();
        services.AddScoped<ITagService, TagService>();

        // External services (HttpClient)
        services.AddHttpClient<INasaService, NasaService>();
        services.AddHttpClient<IAiService, OpenAiService>();

        // Mapster — uses static TypeAdapter API, no DI registration needed

        return services;
    }
}
