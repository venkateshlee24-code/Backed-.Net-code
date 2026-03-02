public sealed record AccountingPostResult(
    bool Found,
    bool AlreadyPosted,
    long VoucherId,
    string VoucherNo
);
