using MyWebApi.Application.Auth;

namespace MyWebApi.Infrastructure.Security;

public interface IJwtTokenService
{
    AuthTokenResponse CreateTokens(
        int userId,
        string email,
        string userName,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions);
}
