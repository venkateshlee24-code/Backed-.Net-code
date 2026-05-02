using MyWebApi.Domain.Models;

namespace MyWebApi.Infrastructure.Repositories;

public interface IAuthRepository
{
    Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<AuthUser?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AuthUser?> GetByActiveRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetActiveRoleCodesAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetActivePermissionCodesAsync(int userId, CancellationToken cancellationToken);
    Task StoreRefreshTokenAsync(
        int userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken);
    Task<bool> RevokeRefreshTokenAsync(int userId, string tokenHash, CancellationToken cancellationToken);
    Task<bool> UpdatePasswordHashAsync(int userId, string passwordHash, CancellationToken cancellationToken);
}
