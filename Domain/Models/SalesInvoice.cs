public sealed record SalesInvoice(
    long InvoiceId,
    string InvoiceNo,
    DateOnly InvoiceDate,
    string CustomerName,
    decimal SubTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Status,
    long? VoucherId
);
