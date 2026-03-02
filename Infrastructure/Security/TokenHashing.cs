using System.Security.Cryptography;
using System.Text;

namespace MyWebApi.Infrastructure.Security;

public static class TokenHashing
{
    public static string Sha256(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
