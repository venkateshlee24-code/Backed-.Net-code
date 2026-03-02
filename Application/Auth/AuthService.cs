using Microsoft.Extensions.Options;
using MyWebApi.Infrastructure.Repositories;
using MyWebApi.Infrastructure.Security;

namespace MyWebApi.Application.Auth;

public sealed class AuthService(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptionsAccessor) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptionsAccessor.Value;

    public async Task<AuthTokenResponse?> LoginAsync(
        LoginRequest request,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var user = await authRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordValid = passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            return null;
        }

        return await IssueTokensAsync(user, createdByIp, userAgent, cancellationToken);
    }

    public async Task<AuthTokenResponse?> RefreshAsync(
        RefreshTokenRequest request,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenHash = TokenHashing.Sha256(request.RefreshToken.Trim());
        var user = await authRepository.GetByActiveRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var revoked = await authRepository.RevokeRefreshTokenAsync(user.Id, tokenHash, cancellationToken);
        if (!revoked)
        {
            return null;
        }

        return await IssueTokensAsync(user, createdByIp, userAgent, cancellationToken);
    }

    public async Task<bool> LogoutAsync(int userId, string refreshToken, CancellationToken cancellationToken)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var refreshTokenHash = TokenHashing.Sha256(refreshToken.Trim());
        return await authRepository.RevokeRefreshTokenAsync(userId, refreshTokenHash, cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (userId <= 0 ||
            string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("Current password and new password are required.");
        }

        ValidatePassword(request.NewPassword);

        var user = await authRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return false;
        }

        var currentValid = passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!currentValid)
        {
            return false;
        }

        var newPasswordHash = passwordHasher.Hash(request.NewPassword);
        return await authRepository.UpdatePasswordHashAsync(userId, newPasswordHash, cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("User id and new password are required.");
        }

        ValidatePassword(request.NewPassword);

        var newPasswordHash = passwordHasher.Hash(request.NewPassword);
        return await authRepository.UpdatePasswordHashAsync(request.UserId, newPasswordHash, cancellationToken);
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(
        Domain.Models.AuthUser user,
        string? createdByIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var roles = await authRepository.GetActiveRoleCodesAsync(user.Id, cancellationToken);
        var permissions = await authRepository.GetActivePermissionCodesAsync(user.Id, cancellationToken);
        var tokenResponse = jwtTokenService.CreateTokens(user.Id, user.Email, user.UserName, roles, permissions);

        var refreshTokenHash = TokenHashing.Sha256(tokenResponse.RefreshToken);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        await authRepository.StoreRefreshTokenAsync(
            userId: user.Id,
            tokenHash: refreshTokenHash,
            expiresAtUtc: refreshTokenExpiry,
            createdByIp: createdByIp,
            userAgent: userAgent,
            cancellationToken: cancellationToken);

        return tokenResponse;
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters.");
        }
    }
}
