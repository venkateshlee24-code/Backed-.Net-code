using Microsoft.AspNetCore.Mvc;
using MyWebApi.Infrastructure.Data;

namespace MyWebApi.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(IDbConnectionFactory connectionFactory) : ControllerBase
{
    [HttpGet("db")]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT DB_NAME();";

        var dbName = (string?)await command.ExecuteScalarAsync(cancellationToken);

        return Ok(new
        {
            connected = true,
            database = dbName
        });
    }
}
