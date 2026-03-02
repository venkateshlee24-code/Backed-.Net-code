namespace MyWebApi.Application.Auth;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    string TokenType = "Bearer"
);
