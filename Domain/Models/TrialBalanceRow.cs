public sealed record TrialBalanceRow(
    long AccountId,
    string AccountCode,
    string AccountName,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal NetBalance
);
