namespace MyWebApi.Application.Auth;

public sealed record RefreshTokenRequest(
    string RefreshToken
);
