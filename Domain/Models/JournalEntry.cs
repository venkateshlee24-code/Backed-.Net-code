public sealed record JournalEntry(
    long JournalId,
    string JournalNo,
    DateOnly JournalDate,
    string Status,
    string? Narration,
    long? VoucherId
);
