public sealed class PaymentCreateRequest
{
    public DateOnly PaymentDate { get; init; }
    public string PartyType { get; init; } = default!;
    public string PartyName { get; init; } = default!;
    public string PaymentType { get; init; } = default!;
    public long OffsetAccountId { get; init; }
    public long CashBankAccountId { get; init; }
    public decimal Amount { get; init; }
    public string? Narration { get; init; }
}
