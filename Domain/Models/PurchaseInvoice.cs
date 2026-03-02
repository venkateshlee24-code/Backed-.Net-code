public sealed record PurchaseInvoice(
    long InvoiceId,
    string InvoiceNo,
    DateOnly InvoiceDate,
    string VendorName,
    decimal SubTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Status,
    long? VoucherId
);
