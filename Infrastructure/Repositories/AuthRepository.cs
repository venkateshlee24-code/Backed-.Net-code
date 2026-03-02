using Microsoft.Data.SqlClient;
using MyWebApi.Domain.Models;
using MyWebApi.Infrastructure.Data;

namespace MyWebApi.Infrastructure.Repositories;

public sealed class AuthRepository(IDbConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<AuthUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) Id, Email, UserName, PasswordHash, IsActive
            FROM dbo.Users
            WHERE Email = @Email;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new AuthUser(
            Id: reader.GetInt32(0),
            Email: reader.GetString(1),
            UserName: reader.GetString(2),
            PasswordHash: reader.GetString(3),
            IsActive: reader.GetBoolean(4)
        );
    }

    public async Task<IReadOnlyList<string>> GetActiveRoleCodesAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT r.RoleCode
            FROM dbo.UserRoleAssignments ura
            INNER JOIN dbo.Roles r ON r.Id = ura.RoleId
            WHERE ura.UserId = @UserId
              AND ura.IsActive = 1
              AND r.IsActive = 1;
            """;

        var roles = new List<string>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

    public async Task<AuthUser?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) Id, Email, UserName, PasswordHash, IsActive
            FROM dbo.Users
            WHERE Id = @Id;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new AuthUser(
            Id: reader.GetInt32(0),
            Email: reader.GetString(1),
            UserName: reader.GetString(2),
            PasswordHash: reader.GetString(3),
            IsActive: reader.GetBoolean(4)
        );
    }

    public async Task<AuthUser?> GetByActiveRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) u.Id, u.Email, u.UserName, u.PasswordHash, u.IsActive
            FROM dbo.RefreshTokens rt
            INNER JOIN dbo.Users u ON u.Id = rt.UserId
            WHERE rt.TokenHash = @TokenHash
              AND rt.RevokedAtUtc IS NULL
              AND rt.ExpiresAtUtc > SYSUTCDATETIME()
              AND u.IsActive = 1;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TokenHash", tokenHash);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new AuthUser(
            Id: reader.GetInt32(0),
            Email: reader.GetString(1),
            UserName: reader.GetString(2),
            PasswordHash: reader.GetString(3),
            IsActive: reader.GetBoolean(4)
        );
    }

    public async Task<IReadOnlyList<string>> GetActivePermissionCodesAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT CONCAT(m.ModuleCode, ':', r.RoleCode) AS PermissionCode
            FROM dbo.UserRoleAssignments ura
            INNER JOIN dbo.Roles r ON r.Id = ura.RoleId
            INNER JOIN dbo.Modules m ON m.Id = ura.ModuleId
            WHERE ura.UserId = @UserId
              AND ura.IsActive = 1
              AND r.IsActive = 1;
            """;

        var permissions = new List<string>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            permissions.Add(reader.GetString(0));
        }

        return permissions;
    }

    public async Task StoreRefreshTokenAsync(
        int userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.RefreshTokens (UserId, TokenHash, ExpiresAtUtc, CreatedByIp, UserAgent)
            VALUES (@UserId, @TokenHash, @ExpiresAtUtc, @CreatedByIp, @UserAgent);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@TokenHash", tokenHash);
        command.Parameters.AddWithValue("@ExpiresAtUtc", expiresAtUtc);
        command.Parameters.AddWithValue("@CreatedByIp", (object?)createdByIp ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserAgent", (object?)userAgent ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(int userId, string tokenHash, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.RefreshTokens
            SET RevokedAtUtc = SYSUTCDATETIME()
            WHERE UserId = @UserId
              AND TokenHash = @TokenHash
              AND RevokedAtUtc IS NULL
              AND ExpiresAtUtc > SYSUTCDATETIME();
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@TokenHash", tokenHash);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> UpdatePasswordHashAsync(int userId, string passwordHash, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users
            SET PasswordHash = @PasswordHash,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId
              AND IsActive = 1;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }
}
