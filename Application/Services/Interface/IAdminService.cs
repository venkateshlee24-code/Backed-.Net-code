public interface IAdminService
{
    Task<IReadOnlyList<AdminLoginSummary>> GetLoginSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminUserSession>> GetActiveUsersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminUserSession>> GetUserSessionsAsync(int userId, CancellationToken cancellationToken);
}
