public sealed record LedgerAccount(
    long AccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    bool IsActive
);
