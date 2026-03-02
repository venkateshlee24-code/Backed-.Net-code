using Microsoft.Data.SqlClient;

namespace MyWebApi.Infrastructure.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

    public SqlConnection CreateConnection() => new(_connectionString);
}
