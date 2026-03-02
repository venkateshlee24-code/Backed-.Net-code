public sealed record JournalEntryLine(
    long AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string? LineNarration
);
