public sealed class JournalLineRequest
{
    public long AccountId { get; init; }
    public decimal DebitAmount { get; init; }
    public decimal CreditAmount { get; init; }
    public string? LineNarration { get; init; }
}
