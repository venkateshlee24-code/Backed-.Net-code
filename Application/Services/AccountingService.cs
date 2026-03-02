namespace MyWebApi.Application.Services;

public sealed class AccountingService(IAccountingRepository repository) : IAccountingService
{
    public Task<IReadOnlyList<LedgerAccount>> GetLedgerAccountsAsync(CancellationToken cancellationToken)
        => repository.GetLedgerAccountsAsync(cancellationToken);

    public Task<long> CreateSalesInvoiceDraftAsync(
        SalesInvoiceCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        ValidateSalesInvoiceRequest(request);
        return repository.CreateSalesInvoiceDraftAsync(request, createdBy, cancellationToken);
    }

    public Task<long> CreatePurchaseInvoiceDraftAsync(
        PurchaseInvoiceCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        ValidatePurchaseInvoiceRequest(request);
        return repository.CreatePurchaseInvoiceDraftAsync(request, createdBy, cancellationToken);
    }

    public Task<long> CreatePaymentDraftAsync(
        PaymentCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        ValidatePaymentRequest(request);
        return repository.CreatePaymentDraftAsync(request, createdBy, cancellationToken);
    }

    public Task<long> CreateJournalDraftAsync(
        JournalCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        ValidateJournalRequest(request);
        return repository.CreateJournalDraftAsync(request, createdBy, cancellationToken);
    }

    public Task<AccountingPostResult> PostSalesInvoiceAsync(
        long invoiceId,
        int postedBy,
        CancellationToken cancellationToken)
        => invoiceId <= 0
            ? Task.FromResult(new AccountingPostResult(false, false, 0, string.Empty))
            : repository.PostSalesInvoiceAsync(invoiceId, postedBy, cancellationToken);

    public Task<AccountingPostResult> PostPurchaseInvoiceAsync(
        long invoiceId,
        int postedBy,
        CancellationToken cancellationToken)
        => invoiceId <= 0
            ? Task.FromResult(new AccountingPostResult(false, false, 0, string.Empty))
            : repository.PostPurchaseInvoiceAsync(invoiceId, postedBy, cancellationToken);

    public Task<AccountingPostResult> PostPaymentAsync(
        long paymentId,
        int postedBy,
        CancellationToken cancellationToken)
        => paymentId <= 0
            ? Task.FromResult(new AccountingPostResult(false, false, 0, string.Empty))
            : repository.PostPaymentAsync(paymentId, postedBy, cancellationToken);

    public Task<AccountingPostResult> PostJournalAsync(
        long journalId,
        int postedBy,
        CancellationToken cancellationToken)
        => journalId <= 0
            ? Task.FromResult(new AccountingPostResult(false, false, 0, string.Empty))
            : repository.PostJournalAsync(journalId, postedBy, cancellationToken);

    public Task<Voucher?> GetVoucherByIdAsync(long voucherId, CancellationToken cancellationToken)
        => voucherId <= 0
            ? Task.FromResult<Voucher?>(null)
            : repository.GetVoucherByIdAsync(voucherId, cancellationToken);

    public Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate > toDate)
        {
            throw new ArgumentException("From date cannot be after to date.");
        }

        return repository.GetTrialBalanceAsync(fromDate, toDate, cancellationToken);
    }

    public Task<IReadOnlyList<LedgerEntryRow>> GetLedgerAsync(
        long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (accountId <= 0)
        {
            return Task.FromResult<IReadOnlyList<LedgerEntryRow>>([]);
        }

        if (fromDate > toDate)
        {
            throw new ArgumentException("From date cannot be after to date.");
        }

        return repository.GetLedgerAsync(accountId, fromDate, toDate, cancellationToken);
    }

    private static void ValidateSalesInvoiceRequest(SalesInvoiceCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new ArgumentException("Customer name is required.");
        }

        if (request.ReceivableAccountId <= 0 || request.RevenueAccountId <= 0)
        {
            throw new ArgumentException("Receivable and revenue accounts are required.");
        }

        if (request.TaxTotal > 0 && request.TaxAccountId is null or <= 0)
        {
            throw new ArgumentException("Tax account is required when tax total is greater than zero.");
        }

        if (request.SubTotal <= 0 || request.TaxTotal < 0)
        {
            throw new ArgumentException("Amounts are invalid.");
        }
    }

    private static void ValidatePurchaseInvoiceRequest(PurchaseInvoiceCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VendorName))
        {
            throw new ArgumentException("Vendor name is required.");
        }

        if (request.PayableAccountId <= 0 || request.ExpenseAccountId <= 0)
        {
            throw new ArgumentException("Payable and expense accounts are required.");
        }

        if (request.TaxTotal > 0 && request.TaxAccountId is null or <= 0)
        {
            throw new ArgumentException("Tax account is required when tax total is greater than zero.");
        }

        if (request.SubTotal <= 0 || request.TaxTotal < 0)
        {
            throw new ArgumentException("Amounts are invalid.");
        }
    }

    private static void ValidatePaymentRequest(PaymentCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PartyType) || string.IsNullOrWhiteSpace(request.PartyName))
        {
            throw new ArgumentException("Party type and party name are required.");
        }

        if (!request.PartyType.Equals("Customer", StringComparison.OrdinalIgnoreCase) &&
            !request.PartyType.Equals("Vendor", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Party type must be Customer or Vendor.");
        }

        if (!request.PaymentType.Equals("Receipt", StringComparison.OrdinalIgnoreCase) &&
            !request.PaymentType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Payment type must be Receipt or Payment.");
        }

        if (request.OffsetAccountId <= 0 || request.CashBankAccountId <= 0)
        {
            throw new ArgumentException("Offset account and cash/bank account are required.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }
    }

    private static void ValidateJournalRequest(JournalCreateRequest request)
    {
        if (request.Lines.Count < 2)
        {
            throw new ArgumentException("At least two journal lines are required.");
        }

        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (var line in request.Lines)
        {
            if (line.AccountId <= 0)
            {
                throw new ArgumentException("Valid account id is required in journal lines.");
            }

            if (line.DebitAmount < 0 || line.CreditAmount < 0)
            {
                throw new ArgumentException("Debit and credit cannot be negative.");
            }

            if ((line.DebitAmount > 0 && line.CreditAmount > 0) ||
                (line.DebitAmount == 0 && line.CreditAmount == 0))
            {
                throw new ArgumentException("Each journal line must have either debit or credit.");
            }

            totalDebit += line.DebitAmount;
            totalCredit += line.CreditAmount;
        }

        if (totalDebit != totalCredit)
        {
            throw new ArgumentException("Journal entry is not balanced.");
        }
    }
}
