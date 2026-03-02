public sealed class PurchaseInvoiceCreateRequest
{
    public DateOnly InvoiceDate { get; init; }
    public string VendorName { get; init; } = default!;
    public long PayableAccountId { get; init; }
    public long ExpenseAccountId { get; init; }
    public long? TaxAccountId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public string? Narration { get; init; }
}
