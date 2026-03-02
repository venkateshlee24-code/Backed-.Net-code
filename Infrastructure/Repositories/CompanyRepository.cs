using Microsoft.Data.SqlClient;
using MyWebApi.Application.Contracts;
using MyWebApi.Domain.Models;
using MyWebApi.Infrastructure.Data;

namespace MyWebApi.Infrastructure.Repositories;

public sealed class CompanyRepository(IDbConnectionFactory connectionFactory)
    : ICompanyRepository
{
    public async Task<IReadOnlyList<Company>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT CompanyId, CompanyCode, CompanyName, BaseCurrencyId, IsActive
            FROM dbo.Companies
            ORDER BY CompanyName;
            """;

        var results = new List<Company>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapCompany(reader));
        }

        return results;
    }

    public async Task<Company?> GetByIdAsync(
        long companyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT CompanyId, CompanyCode, CompanyName, BaseCurrencyId, IsActive
            FROM dbo.Companies
            WHERE CompanyId = @CompanyId;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CompanyId", companyId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapCompany(reader);
    }

    public async Task<long> CreateAsync(
        CompanyCreateRequest request,
        CancellationToken cancellationToken)
    {
      const string sql = """
    INSERT INTO dbo.Companies
        (CompanyCode, CompanyName, BaseCurrencyId, IsActive, CreatedAt, CreatedBy)
    VALUES
        (@CompanyCode, @CompanyName, @BaseCurrencyId, 1, SYSUTCDATETIME(), @CreatedBy);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
    """;
  await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@CompanyCode", request.CompanyCode.Trim());
        command.Parameters.AddWithValue("@CompanyName", request.CompanyName.Trim());
        command.Parameters.AddWithValue("@BaseCurrencyId", request.BaseCurrencyId);
        command.Parameters.AddWithValue("@CreatedBy", request.CreatedBy);
       
        var createdId = (long?)await command.ExecuteScalarAsync(cancellationToken);

        if (createdId is null or <= 0)
            throw new InvalidOperationException("Failed to create company.");

        return createdId.Value;
    }

    public async Task<bool> DeactivateAsync(
        long companyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Companies
            SET IsActive = 0
            WHERE CompanyId = @CompanyId;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CompanyId", companyId);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    private static Company MapCompany(SqlDataReader reader) =>
        new(
            CompanyId: reader.GetInt64(0),
            CompanyCode: reader.GetString(1),
            CompanyName: reader.GetString(2),
            BaseCurrencyId: reader.GetInt32(3),
            IsActive: reader.GetBoolean(4)
        );

    public Task<bool> UpdateAsync(long id, CompanyCreateRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
