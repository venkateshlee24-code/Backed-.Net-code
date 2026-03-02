public sealed class SalesInvoiceCreateRequest
{
    public DateOnly InvoiceDate { get; init; }
    public string CustomerName { get; init; } = default!;
    public long ReceivableAccountId { get; init; }
    public long RevenueAccountId { get; init; }
    public long? TaxAccountId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public string? Narration { get; init; }
}
