using Microsoft.Data.SqlClient;
using MyWebApi.Application.Contracts;
using MyWebApi.Domain.Models;
using MyWebApi.Infrastructure.Data;

namespace MyWebApi.Infrastructure.Repositories;

public sealed class EmployeeRepository(IDbConnectionFactory connectionFactory) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, EmployeeCode, FullName, Email, DepartmentCode, JoiningDate, IsActive
            FROM dbo.Employees
            ORDER BY Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var results = new List<Employee>(pageSize);
        var offset = (page - 1) * pageSize;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", offset);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapEmployee(reader));
        }

        return results;
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, EmployeeCode, FullName, Email, DepartmentCode, JoiningDate, IsActive
            FROM dbo.Employees
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

        return MapEmployee(reader);
    }

  public async Task<int> CreateAsync(
    EmployeeCreateRequest request,
    CancellationToken cancellationToken)
{
    const string sql = """
        BEGIN TRAN;

        INSERT INTO dbo.Employees
            (EmployeeCode, FullName, Email, DepartmentCode, JoiningDate, IsActive, CreatedAt)
        VALUES
            (@EmployeeCode, @FullName, @Email, @DepartmentCode, @JoiningDate, 1, SYSUTCDATETIME());

        DECLARE @EmployeeId INT = SCOPE_IDENTITY();

        INSERT INTO dbo.Users
            (UserCode, UserName, Email, PasswordHash, IsActive, CreatedAt)
        VALUES
            (@UserCode, @FullName, @Email, @PasswordHash, 1, SYSUTCDATETIME());

        COMMIT TRAN;

        SELECT @EmployeeId;
        """;

    // Default user code example: EMP001
    var userCode = "EMP" + request.EmployeeCode;

    var passwordHash =
        BCrypt.Net.BCrypt.HashPassword(request.Password);

    await using var connection = connectionFactory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await using var command = new SqlCommand(sql, connection);

    command.Parameters.AddWithValue("@EmployeeCode", request.EmployeeCode.Trim());
    command.Parameters.AddWithValue("@FullName", request.FullName.Trim());
    command.Parameters.AddWithValue("@Email", request.Email.Trim());
    command.Parameters.AddWithValue("@DepartmentCode", request.DepartmentCode.Trim());
    command.Parameters.AddWithValue("@JoiningDate",
        request.JoiningDate.ToDateTime(TimeOnly.MinValue));

    command.Parameters.AddWithValue("@UserCode", userCode);
    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

    var createdId = (int?)await command.ExecuteScalarAsync(cancellationToken);

    if (createdId is null or <= 0)
        throw new InvalidOperationException("Failed to create employee.");

    return createdId.Value;
}
    public async Task<bool> UpdateAsync(int id, EmployeeUpdateRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Employees
            SET FullName = @FullName,
                Email = @Email,
                DepartmentCode = @DepartmentCode,
                IsActive = @IsActive
            WHERE Id = @Id;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@FullName", request.FullName.Trim());
        command.Parameters.AddWithValue("@Email", request.Email.Trim());
        command.Parameters.AddWithValue("@DepartmentCode", request.DepartmentCode.Trim());
        command.Parameters.AddWithValue("@IsActive", request.IsActive);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Employees
            SET IsActive = 0
            WHERE Id = @Id;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) 1
            FROM dbo.Employees
            WHERE Email = @Email
              AND (@ExcludeEmployeeId IS NULL OR Id <> @ExcludeEmployeeId);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@ExcludeEmployeeId", (object?)excludeEmployeeId ?? DBNull.Value);

        var exists = await command.ExecuteScalarAsync(cancellationToken);
        return exists is not null;
    }

    private static Employee MapEmployee(SqlDataReader reader) =>
        new(
            Id: reader.GetInt32(0),
            EmployeeCode: reader.GetString(1),
            FullName: reader.GetString(2),
            Email: reader.GetString(3),
            DepartmentCode: reader.GetString(4),
            JoiningDate: DateOnly.FromDateTime(reader.GetDateTime(5)),
            IsActive: reader.GetBoolean(6)
        );
}
