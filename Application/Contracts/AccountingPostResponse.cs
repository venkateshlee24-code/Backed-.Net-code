public sealed record AccountingPostResponse(
    long VoucherId,
    string VoucherNo,
    bool AlreadyPosted
);
