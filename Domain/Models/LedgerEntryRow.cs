public sealed record LedgerEntryRow(
    DateTime PostedAtUtc,
    string VoucherNo,
    string VoucherType,
    long VoucherId,
    string? Narration,
    decimal DebitAmount,
    decimal CreditAmount
);
