public sealed record Voucher(
    long VoucherId,
    string VoucherNo,
    string VoucherType,
    DateOnly VoucherDate,
    string Status,
    string? Narration,
    string? SourceType,
    long? SourceId,
    DateTime PostedAtUtc,
    IReadOnlyList<VoucherLine> Lines
);
