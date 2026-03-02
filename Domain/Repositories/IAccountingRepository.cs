public interface IAccountingRepository
{
    Task<IReadOnlyList<LedgerAccount>> GetLedgerAccountsAsync(CancellationToken cancellationToken);

    Task<long> CreateSalesInvoiceDraftAsync(
        SalesInvoiceCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken);

    Task<long> CreatePurchaseInvoiceDraftAsync(
        PurchaseInvoiceCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken);

    Task<long> CreatePaymentDraftAsync(
        PaymentCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken);

    Task<long> CreateJournalDraftAsync(
        JournalCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken);

    Task<AccountingPostResult> PostSalesInvoiceAsync(
        long invoiceId,
        int postedBy,
        CancellationToken cancellationToken);

    Task<AccountingPostResult> PostPurchaseInvoiceAsync(
        long invoiceId,
        int postedBy,
        CancellationToken cancellationToken);

    Task<AccountingPostResult> PostPaymentAsync(
        long paymentId,
        int postedBy,
        CancellationToken cancellationToken);

    Task<AccountingPostResult> PostJournalAsync(
        long journalId,
        int postedBy,
        CancellationToken cancellationToken);

    Task<Voucher?> GetVoucherByIdAsync(long voucherId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LedgerEntryRow>> GetLedgerAsync(
        long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);
}
