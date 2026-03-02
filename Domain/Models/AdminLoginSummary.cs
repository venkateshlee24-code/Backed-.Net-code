public sealed record AdminLoginSummary(
    int UserId,
    string UserName,
    int TotalLogins,
    int TotalMinutesLoggedIn,
    DateTime? LastLoginUtc
);
