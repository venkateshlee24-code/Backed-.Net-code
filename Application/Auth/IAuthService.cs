namespace MyWebApi.Application.Auth;

public interface IAuthService
{
    Task<AuthTokenResponse?> LoginAsync(
        LoginRequest request,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<AuthTokenResponse?> RefreshAsync(
        RefreshTokenRequest request,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<bool> LogoutAsync(
        int userId,
        string refreshToken,
        CancellationToken cancellationToken);

    Task<bool> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken);

    Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken);
}
