using Microsoft.Data.SqlClient;
using MyWebApi.Domain.Models;
using MyWebApi.Infrastructure.Data;

namespace MyWebApi.Infrastructure.Repositories;

public sealed class AdminRepository(IDbConnectionFactory connectionFactory)
    : IAdminRepository
{
    public async Task<IReadOnlyList<AdminLoginSummary>> GetLoginSummaryAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT 
                u.Id,
                u.UserName,
                COUNT(rt.Id) AS TotalLogins,
                SUM(
                    CASE 
                        WHEN rt.RevokedAtUtc IS NOT NULL 
                        THEN DATEDIFF(MINUTE, rt.CreatedAtUtc, rt.RevokedAtUtc)
                        ELSE 0
                    END
                ) AS TotalMinutesLoggedIn,
                MAX(rt.CreatedAtUtc) AS LastLoginUtc
            FROM dbo.Users u
            LEFT JOIN dbo.RefreshTokens rt ON u.Id = rt.UserId
            GROUP BY u.Id, u.UserName
            ORDER BY TotalLogins DESC;
            """;

        var results = new List<AdminLoginSummary>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AdminLoginSummary(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetDateTime(4)
            ));
        }

        return results;
    }

    public async Task<IReadOnlyList<AdminUserSession>> GetActiveUsersAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT 
                rt.Id,
                rt.CreatedAtUtc,
                rt.RevokedAtUtc,
                rt.CreatedByIp,
                rt.UserAgent,
                CASE 
                    WHEN rt.RevokedAtUtc IS NULL 
                         AND rt.ExpiresAtUtc > SYSUTCDATETIME()
                    THEN 1 ELSE 0
                END AS IsActive
            FROM dbo.RefreshTokens rt
            WHERE rt.RevokedAtUtc IS NULL
              AND rt.ExpiresAtUtc > SYSUTCDATETIME();
            """;

        var results = new List<AdminUserSession>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AdminUserSession(
                reader.GetInt64(0),
                reader.GetDateTime(1),
                reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5) == 1
            ));
        }

        return results;
    }

    public async Task<IReadOnlyList<AdminUserSession>> GetUserSessionsAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT 
                Id,
                CreatedAtUtc,
                RevokedAtUtc,
                CreatedByIp,
                UserAgent,
                CASE 
                    WHEN RevokedAtUtc IS NULL 
                         AND ExpiresAtUtc > SYSUTCDATETIME()
                    THEN 1 ELSE 0
                END AS IsActive
            FROM dbo.RefreshTokens
            WHERE UserId = @UserId
            ORDER BY CreatedAtUtc DESC;
            """;

        var results = new List<AdminUserSession>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AdminUserSession(
                reader.GetInt64(0),
                reader.GetDateTime(1),
                reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5) == 1
            ));
        }

        return results;
    }
}
