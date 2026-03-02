using Microsoft.Data.SqlClient;

namespace MyWebApi.Infrastructure.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
