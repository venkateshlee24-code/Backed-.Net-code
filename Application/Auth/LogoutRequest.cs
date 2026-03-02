namespace MyWebApi.Application.Auth;

public sealed record LogoutRequest(
    string RefreshToken
);
