using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.Features.Auth;
using SpaceExplorer.Application.Features.Auth.Dtos;
using SpaceExplorer.Domain.Entities;
using SpaceExplorer.Infrastructure.Common;
using SpaceExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace SpaceExplorer.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : GenericRepository<User, AuthResponse, Guid>(context), IUserRepository
{
    public async Task<AuthResponse?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);
        return user?.Adapt<AuthResponse>();
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.AnyAsync(u => u.Email == email, ct);
}
