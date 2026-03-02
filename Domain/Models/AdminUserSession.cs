public sealed record AdminUserSession(
    long RefreshTokenId,
    DateTime LoginAtUtc,
    DateTime? LogoutAtUtc,
    string? IpAddress,
    string? UserAgent,
    bool IsActive
);
