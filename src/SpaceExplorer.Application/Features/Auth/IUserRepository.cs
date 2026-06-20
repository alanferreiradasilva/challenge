using SpaceExplorer.Application.Features.Auth.Dtos;
using SpaceExplorer.Application.Common.Interfaces;

namespace SpaceExplorer.Application.Features.Auth;

public interface IUserRepository : IGenericRepository<AuthResponse, Guid>
{
    Task<AuthResponse?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
