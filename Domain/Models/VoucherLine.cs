public sealed record VoucherLine(
    long VoucherLineId,
    long VoucherId,
    long AccountId,
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string? LineNarration
);
