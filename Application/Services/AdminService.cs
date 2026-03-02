public sealed class AdminService(IAdminRepository repo) : IAdminService
{
    public Task<IReadOnlyList<AdminLoginSummary>> GetLoginSummaryAsync(
        CancellationToken cancellationToken)
        => repo.GetLoginSummaryAsync(cancellationToken);

    public Task<IReadOnlyList<AdminUserSession>> GetActiveUsersAsync(
        CancellationToken cancellationToken)
        => repo.GetActiveUsersAsync(cancellationToken);

    public Task<IReadOnlyList<AdminUserSession>> GetUserSessionsAsync(
        int userId,
        CancellationToken cancellationToken)
        => repo.GetUserSessionsAsync(userId, cancellationToken);
}
