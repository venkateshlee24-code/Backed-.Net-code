using Microsoft.Data.SqlClient;
using MyWebApi.Infrastructure.Data;

namespace MyWebApi.Infrastructure.Repositories;

public sealed class AccountingRepository(IDbConnectionFactory connectionFactory) : IAccountingRepository
{
    public async Task<IReadOnlyList<LedgerAccount>> GetLedgerAccountsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT AccountId, AccountCode, AccountName, AccountType, IsActive
            FROM dbo.LedgerAccounts
            WHERE IsActive = 1
            ORDER BY AccountCode;
            """;

        var results = new List<LedgerAccount>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new LedgerAccount(
                AccountId: reader.GetInt64(0),
                AccountCode: reader.GetString(1),
                AccountName: reader.GetString(2),
                AccountType: reader.GetString(3),
                IsActive: reader.GetBoolean(4)));
        }

        return results;
    }

    public async Task<long> CreateSalesInvoiceDraftAsync(
        SalesInvoiceCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.SalesInvoices
                (InvoiceNo, InvoiceDate, CustomerName, ReceivableAccountId, RevenueAccountId, TaxAccountId, SubTotal, TaxTotal, GrandTotal, Status, Narration, CreatedBy)
            VALUES
                (@InvoiceNo, @InvoiceDate, @CustomerName, @ReceivableAccountId, @RevenueAccountId, @TaxAccountId, @SubTotal, @TaxTotal, @GrandTotal, 'Draft', @Narration, @CreatedBy);

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        var invoiceNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "SI", cancellationToken);

        await using var command = new SqlCommand(sql, connection, sqlTransaction);
        command.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
        command.Parameters.AddWithValue("@InvoiceDate", request.InvoiceDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@CustomerName", request.CustomerName.Trim());
        command.Parameters.AddWithValue("@ReceivableAccountId", request.ReceivableAccountId);
        command.Parameters.AddWithValue("@RevenueAccountId", request.RevenueAccountId);
        command.Parameters.AddWithValue("@TaxAccountId", (object?)request.TaxAccountId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubTotal", request.SubTotal);
        command.Parameters.AddWithValue("@TaxTotal", request.TaxTotal);
        command.Parameters.AddWithValue("@GrandTotal", request.SubTotal + request.TaxTotal);
        command.Parameters.AddWithValue("@Narration", (object?)request.Narration ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", createdBy <= 0 ? DBNull.Value : createdBy);

        var id = (long?)await command.ExecuteScalarAsync(cancellationToken);
        await sqlTransaction.CommitAsync(cancellationToken);

        if (id is null or <= 0)
        {
            throw new InvalidOperationException("Failed to create sales invoice draft.");
        }

        return id.Value;
    }

    public async Task<long> CreatePurchaseInvoiceDraftAsync(
        PurchaseInvoiceCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.PurchaseInvoices
                (InvoiceNo, InvoiceDate, VendorName, PayableAccountId, ExpenseAccountId, TaxAccountId, SubTotal, TaxTotal, GrandTotal, Status, Narration, CreatedBy)
            VALUES
                (@InvoiceNo, @InvoiceDate, @VendorName, @PayableAccountId, @ExpenseAccountId, @TaxAccountId, @SubTotal, @TaxTotal, @GrandTotal, 'Draft', @Narration, @CreatedBy);

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        var invoiceNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "PI", cancellationToken);

        await using var command = new SqlCommand(sql, connection, sqlTransaction);
        command.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
        command.Parameters.AddWithValue("@InvoiceDate", request.InvoiceDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@VendorName", request.VendorName.Trim());
        command.Parameters.AddWithValue("@PayableAccountId", request.PayableAccountId);
        command.Parameters.AddWithValue("@ExpenseAccountId", request.ExpenseAccountId);
        command.Parameters.AddWithValue("@TaxAccountId", (object?)request.TaxAccountId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubTotal", request.SubTotal);
        command.Parameters.AddWithValue("@TaxTotal", request.TaxTotal);
        command.Parameters.AddWithValue("@GrandTotal", request.SubTotal + request.TaxTotal);
        command.Parameters.AddWithValue("@Narration", (object?)request.Narration ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", createdBy <= 0 ? DBNull.Value : createdBy);

        var id = (long?)await command.ExecuteScalarAsync(cancellationToken);
        await sqlTransaction.CommitAsync(cancellationToken);

        if (id is null or <= 0)
        {
            throw new InvalidOperationException("Failed to create purchase invoice draft.");
        }

        return id.Value;
    }

    public async Task<long> CreatePaymentDraftAsync(
        PaymentCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Payments
                (PaymentNo, PaymentDate, PartyType, PartyName, PaymentType, OffsetAccountId, CashBankAccountId, Amount, Status, Narration, CreatedBy)
            VALUES
                (@PaymentNo, @PaymentDate, @PartyType, @PartyName, @PaymentType, @OffsetAccountId, @CashBankAccountId, @Amount, 'Draft', @Narration, @CreatedBy);

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        var paymentNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "PAY", cancellationToken);

        await using var command = new SqlCommand(sql, connection, sqlTransaction);
        command.Parameters.AddWithValue("@PaymentNo", paymentNo);
        command.Parameters.AddWithValue("@PaymentDate", request.PaymentDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@PartyType", request.PartyType.Trim());
        command.Parameters.AddWithValue("@PartyName", request.PartyName.Trim());
        command.Parameters.AddWithValue("@PaymentType", request.PaymentType.Trim());
        command.Parameters.AddWithValue("@OffsetAccountId", request.OffsetAccountId);
        command.Parameters.AddWithValue("@CashBankAccountId", request.CashBankAccountId);
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@Narration", (object?)request.Narration ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", createdBy <= 0 ? DBNull.Value : createdBy);

        var id = (long?)await command.ExecuteScalarAsync(cancellationToken);
        await sqlTransaction.CommitAsync(cancellationToken);

        if (id is null or <= 0)
        {
            throw new InvalidOperationException("Failed to create payment draft.");
        }

        return id.Value;
    }

    public async Task<long> CreateJournalDraftAsync(
        JournalCreateRequest request,
        int createdBy,
        CancellationToken cancellationToken)
    {
        const string insertJournalSql = """
            INSERT INTO dbo.JournalEntries
                (JournalNo, JournalDate, Status, Narration, CreatedBy)
            VALUES
                (@JournalNo, @JournalDate, 'Draft', @Narration, @CreatedBy);

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        const string insertLineSql = """
            INSERT INTO dbo.JournalEntryLines
                (JournalId, AccountId, DebitAmount, CreditAmount, LineNarration)
            VALUES
                (@JournalId, @AccountId, @DebitAmount, @CreditAmount, @LineNarration);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        var journalNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "JRN", cancellationToken);

        await using var journalCommand = new SqlCommand(insertJournalSql, connection, sqlTransaction);
        journalCommand.Parameters.AddWithValue("@JournalNo", journalNo);
        journalCommand.Parameters.AddWithValue("@JournalDate", request.JournalDate.ToDateTime(TimeOnly.MinValue));
        journalCommand.Parameters.AddWithValue("@Narration", (object?)request.Narration ?? DBNull.Value);
        journalCommand.Parameters.AddWithValue("@CreatedBy", createdBy <= 0 ? DBNull.Value : createdBy);

        var journalId = (long?)await journalCommand.ExecuteScalarAsync(cancellationToken);
        if (journalId is null or <= 0)
        {
            throw new InvalidOperationException("Failed to create journal draft.");
        }

        foreach (var line in request.Lines)
        {
            await using var lineCommand = new SqlCommand(insertLineSql, connection, sqlTransaction);
            lineCommand.Parameters.AddWithValue("@JournalId", journalId.Value);
            lineCommand.Parameters.AddWithValue("@AccountId", line.AccountId);
            lineCommand.Parameters.AddWithValue("@DebitAmount", line.DebitAmount);
            lineCommand.Parameters.AddWithValue("@CreditAmount", line.CreditAmount);
            lineCommand.Parameters.AddWithValue("@LineNarration", (object?)line.LineNarration ?? DBNull.Value);
            await lineCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await sqlTransaction.CommitAsync(cancellationToken);
        return journalId.Value;
    }

    public async Task<AccountingPostResult> PostSalesInvoiceAsync(
        long invoiceId,
        int postedBy,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        const string selectSql = """
            SELECT InvoiceDate, ReceivableAccountId, RevenueAccountId, TaxAccountId, SubTotal, TaxTotal, GrandTotal, Status, VoucherId, Narration
            FROM dbo.SalesInvoices WITH (UPDLOCK, HOLDLOCK)
            WHERE InvoiceId = @InvoiceId;
            """;

        await using var selectCommand = new SqlCommand(selectSql, connection, sqlTransaction);
        selectCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AccountingPostResult(false, false, 0, string.Empty);
        }

        var invoiceDate = DateOnly.FromDateTime(reader.GetDateTime(0));
        var receivableAccountId = reader.GetInt64(1);
        var revenueAccountId = reader.GetInt64(2);
        var taxAccountId = reader.IsDBNull(3) ? (long?)null : reader.GetInt64(3);
        var subTotal = reader.GetDecimal(4);
        var taxTotal = reader.GetDecimal(5);
        var grandTotal = reader.GetDecimal(6);
        var status = reader.GetString(7);
        var voucherId = reader.IsDBNull(8) ? (long?)null : reader.GetInt64(8);
        var narration = reader.IsDBNull(9) ? null : reader.GetString(9);
        await reader.CloseAsync();

        if (status.Equals("Posted", StringComparison.OrdinalIgnoreCase) && voucherId.HasValue)
        {
            var existingVoucherNo = await GetVoucherNoAsync(connection, sqlTransaction, voucherId.Value, cancellationToken);
            await sqlTransaction.CommitAsync(cancellationToken);
            return new AccountingPostResult(true, true, voucherId.Value, existingVoucherNo);
        }

        var voucherNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "VCH", cancellationToken);
        var createdVoucherId = await InsertVoucherAsync(
            connection, sqlTransaction, voucherNo, "SI", invoiceDate, narration, "SalesInvoice", invoiceId, postedBy, cancellationToken);

        await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, receivableAccountId, grandTotal, 0, "Sales receivable", cancellationToken);
        await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, revenueAccountId, 0, subTotal, "Sales revenue", cancellationToken);
        if (taxTotal > 0 && taxAccountId.HasValue)
        {
            await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, taxAccountId.Value, 0, taxTotal, "Output tax", cancellationToken);
        }

        const string updateSql = """
            UPDATE dbo.SalesInvoices
            SET Status = 'Posted',
                VoucherId = @VoucherId,
                UpdatedAt = SYSUTCDATETIME(),
                UpdatedBy = @UpdatedBy
            WHERE InvoiceId = @InvoiceId;
            """;
        await using var updateCommand = new SqlCommand(updateSql, connection, sqlTransaction);
        updateCommand.Parameters.AddWithValue("@VoucherId", createdVoucherId);
        updateCommand.Parameters.AddWithValue("@UpdatedBy", postedBy <= 0 ? DBNull.Value : postedBy);
        updateCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);

        await sqlTransaction.CommitAsync(cancellationToken);
        return new AccountingPostResult(true, false, createdVoucherId, voucherNo);
    }

    public async Task<AccountingPostResult> PostPurchaseInvoiceAsync(
        long invoiceId,
        int postedBy,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        const string selectSql = """
            SELECT InvoiceDate, PayableAccountId, ExpenseAccountId, TaxAccountId, SubTotal, TaxTotal, GrandTotal, Status, VoucherId, Narration
            FROM dbo.PurchaseInvoices WITH (UPDLOCK, HOLDLOCK)
            WHERE InvoiceId = @InvoiceId;
            """;

        await using var selectCommand = new SqlCommand(selectSql, connection, sqlTransaction);
        selectCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AccountingPostResult(false, false, 0, string.Empty);
        }

        var invoiceDate = DateOnly.FromDateTime(reader.GetDateTime(0));
        var payableAccountId = reader.GetInt64(1);
        var expenseAccountId = reader.GetInt64(2);
        var taxAccountId = reader.IsDBNull(3) ? (long?)null : reader.GetInt64(3);
        var subTotal = reader.GetDecimal(4);
        var taxTotal = reader.GetDecimal(5);
        var grandTotal = reader.GetDecimal(6);
        var status = reader.GetString(7);
        var voucherId = reader.IsDBNull(8) ? (long?)null : reader.GetInt64(8);
        var narration = reader.IsDBNull(9) ? null : reader.GetString(9);
        await reader.CloseAsync();

        if (status.Equals("Posted", StringComparison.OrdinalIgnoreCase) && voucherId.HasValue)
        {
            var existingVoucherNo = await GetVoucherNoAsync(connection, sqlTransaction, voucherId.Value, cancellationToken);
            await sqlTransaction.CommitAsync(cancellationToken);
            return new AccountingPostResult(true, true, voucherId.Value, existingVoucherNo);
        }

        var voucherNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "VCH", cancellationToken);
        var createdVoucherId = await InsertVoucherAsync(
            connection, sqlTransaction, voucherNo, "PI", invoiceDate, narration, "PurchaseInvoice", invoiceId, postedBy, cancellationToken);

        await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, expenseAccountId, subTotal, 0, "Purchase expense", cancellationToken);
        if (taxTotal > 0 && taxAccountId.HasValue)
        {
            await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, taxAccountId.Value, taxTotal, 0, "Input tax", cancellationToken);
        }
        await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, payableAccountId, 0, grandTotal, "Accounts payable", cancellationToken);

        const string updateSql = """
            UPDATE dbo.PurchaseInvoices
            SET Status = 'Posted',
                VoucherId = @VoucherId,
                UpdatedAt = SYSUTCDATETIME(),
                UpdatedBy = @UpdatedBy
            WHERE InvoiceId = @InvoiceId;
            """;
        await using var updateCommand = new SqlCommand(updateSql, connection, sqlTransaction);
        updateCommand.Parameters.AddWithValue("@VoucherId", createdVoucherId);
        updateCommand.Parameters.AddWithValue("@UpdatedBy", postedBy <= 0 ? DBNull.Value : postedBy);
        updateCommand.Parameters.AddWithValue("@InvoiceId", invoiceId);
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);

        await sqlTransaction.CommitAsync(cancellationToken);
        return new AccountingPostResult(true, false, createdVoucherId, voucherNo);
    }

    public async Task<AccountingPostResult> PostPaymentAsync(
        long paymentId,
        int postedBy,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        const string selectSql = """
            SELECT PaymentDate, PaymentType, OffsetAccountId, CashBankAccountId, Amount, Status, VoucherId, Narration
            FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)
            WHERE PaymentId = @PaymentId;
            """;

        await using var selectCommand = new SqlCommand(selectSql, connection, sqlTransaction);
        selectCommand.Parameters.AddWithValue("@PaymentId", paymentId);
        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AccountingPostResult(false, false, 0, string.Empty);
        }

        var paymentDate = DateOnly.FromDateTime(reader.GetDateTime(0));
        var paymentType = reader.GetString(1);
        var offsetAccountId = reader.GetInt64(2);
        var cashBankAccountId = reader.GetInt64(3);
        var amount = reader.GetDecimal(4);
        var status = reader.GetString(5);
        var voucherId = reader.IsDBNull(6) ? (long?)null : reader.GetInt64(6);
        var narration = reader.IsDBNull(7) ? null : reader.GetString(7);
        await reader.CloseAsync();

        if (status.Equals("Posted", StringComparison.OrdinalIgnoreCase) && voucherId.HasValue)
        {
            var existingVoucherNo = await GetVoucherNoAsync(connection, sqlTransaction, voucherId.Value, cancellationToken);
            await sqlTransaction.CommitAsync(cancellationToken);
            return new AccountingPostResult(true, true, voucherId.Value, existingVoucherNo);
        }

        var voucherNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "VCH", cancellationToken);
        var voucherType = paymentType.Equals("Receipt", StringComparison.OrdinalIgnoreCase) ? "RV" : "PV";
        var createdVoucherId = await InsertVoucherAsync(
            connection, sqlTransaction, voucherNo, voucherType, paymentDate, narration, "Payment", paymentId, postedBy, cancellationToken);

        if (paymentType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
        {
            await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, cashBankAccountId, amount, 0, "Cash/Bank receipt", cancellationToken);
            await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, offsetAccountId, 0, amount, "Offset account", cancellationToken);
        }
        else
        {
            await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, offsetAccountId, amount, 0, "Offset account", cancellationToken);
            await InsertVoucherLineAsync(connection, sqlTransaction, createdVoucherId, cashBankAccountId, 0, amount, "Cash/Bank payment", cancellationToken);
        }

        const string updateSql = """
            UPDATE dbo.Payments
            SET Status = 'Posted',
                VoucherId = @VoucherId,
                UpdatedAt = SYSUTCDATETIME(),
                UpdatedBy = @UpdatedBy
            WHERE PaymentId = @PaymentId;
            """;
        await using var updateCommand = new SqlCommand(updateSql, connection, sqlTransaction);
        updateCommand.Parameters.AddWithValue("@VoucherId", createdVoucherId);
        updateCommand.Parameters.AddWithValue("@UpdatedBy", postedBy <= 0 ? DBNull.Value : postedBy);
        updateCommand.Parameters.AddWithValue("@PaymentId", paymentId);
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);

        await sqlTransaction.CommitAsync(cancellationToken);
        return new AccountingPostResult(true, false, createdVoucherId, voucherNo);
    }

    public async Task<AccountingPostResult> PostJournalAsync(
        long journalId,
        int postedBy,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var sqlTransaction = (SqlTransaction)transaction;

        const string selectSql = """
            SELECT JournalDate, Status, VoucherId, Narration
            FROM dbo.JournalEntries WITH (UPDLOCK, HOLDLOCK)
            WHERE JournalId = @JournalId;
            """;

        await using var selectCommand = new SqlCommand(selectSql, connection, sqlTransaction);
        selectCommand.Parameters.AddWithValue("@JournalId", journalId);
        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AccountingPostResult(false, false, 0, string.Empty);
        }

        var journalDate = DateOnly.FromDateTime(reader.GetDateTime(0));
        var status = reader.GetString(1);
        var voucherId = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);
        var narration = reader.IsDBNull(3) ? null : reader.GetString(3);
        await reader.CloseAsync();

        if (status.Equals("Posted", StringComparison.OrdinalIgnoreCase) && voucherId.HasValue)
        {
            var existingVoucherNo = await GetVoucherNoAsync(connection, sqlTransaction, voucherId.Value, cancellationToken);
            await sqlTransaction.CommitAsync(cancellationToken);
            return new AccountingPostResult(true, true, voucherId.Value, existingVoucherNo);
        }

        var lines = await GetJournalLinesAsync(connection, sqlTransaction, journalId, cancellationToken);
        if (lines.Count < 2)
        {
            throw new InvalidOperationException("Journal has fewer than two lines.");
        }

        var totalDebit = lines.Sum(x => x.DebitAmount);
        var totalCredit = lines.Sum(x => x.CreditAmount);
        if (totalDebit <= 0 || totalDebit != totalCredit)
        {
            throw new InvalidOperationException("Journal entry is not balanced.");
        }

        var voucherNo = await GetNextDocumentNumberAsync(connection, sqlTransaction, "VCH", cancellationToken);
        var createdVoucherId = await InsertVoucherAsync(
            connection, sqlTransaction, voucherNo, "JV", journalDate, narration, "JournalEntry", journalId, postedBy, cancellationToken);

        foreach (var line in lines)
        {
            await InsertVoucherLineAsync(
                connection,
                sqlTransaction,
                createdVoucherId,
                line.AccountId,
                line.DebitAmount,
                line.CreditAmount,
                line.LineNarration,
                cancellationToken);
        }

        const string updateSql = """
            UPDATE dbo.JournalEntries
            SET Status = 'Posted',
                VoucherId = @VoucherId,
                UpdatedAt = SYSUTCDATETIME(),
                UpdatedBy = @UpdatedBy
            WHERE JournalId = @JournalId;
            """;
        await using var updateCommand = new SqlCommand(updateSql, connection, sqlTransaction);
        updateCommand.Parameters.AddWithValue("@VoucherId", createdVoucherId);
        updateCommand.Parameters.AddWithValue("@UpdatedBy", postedBy <= 0 ? DBNull.Value : postedBy);
        updateCommand.Parameters.AddWithValue("@JournalId", journalId);
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);

        await sqlTransaction.CommitAsync(cancellationToken);
        return new AccountingPostResult(true, false, createdVoucherId, voucherNo);
    }

    public async Task<Voucher?> GetVoucherByIdAsync(long voucherId, CancellationToken cancellationToken)
    {
        const string headerSql = """
            SELECT VoucherId, VoucherNo, VoucherType, VoucherDate, Status, Narration, SourceType, SourceId, PostedAtUtc
            FROM dbo.Vouchers
            WHERE VoucherId = @VoucherId;
            """;

        const string linesSql = """
            SELECT vl.VoucherLineId, vl.VoucherId, vl.AccountId, la.AccountCode, la.AccountName, vl.DebitAmount, vl.CreditAmount, vl.LineNarration
            FROM dbo.VoucherLines vl
            INNER JOIN dbo.LedgerAccounts la ON la.AccountId = vl.AccountId
            WHERE vl.VoucherId = @VoucherId
            ORDER BY vl.VoucherLineId;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var headerCommand = new SqlCommand(headerSql, connection);
        headerCommand.Parameters.AddWithValue("@VoucherId", voucherId);
        await using var headerReader = await headerCommand.ExecuteReaderAsync(cancellationToken);
        if (!await headerReader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var header = (
            VoucherId: headerReader.GetInt64(0),
            VoucherNo: headerReader.GetString(1),
            VoucherType: headerReader.GetString(2),
            VoucherDate: DateOnly.FromDateTime(headerReader.GetDateTime(3)),
            Status: headerReader.GetString(4),
            Narration: headerReader.IsDBNull(5) ? null : headerReader.GetString(5),
            SourceType: headerReader.IsDBNull(6) ? null : headerReader.GetString(6),
            SourceId: headerReader.IsDBNull(7) ? (long?)null : headerReader.GetInt64(7),
            PostedAtUtc: headerReader.GetDateTime(8));
        await headerReader.CloseAsync();

        var lines = new List<VoucherLine>();
        await using var linesCommand = new SqlCommand(linesSql, connection);
        linesCommand.Parameters.AddWithValue("@VoucherId", voucherId);
        await using var linesReader = await linesCommand.ExecuteReaderAsync(cancellationToken);
        while (await linesReader.ReadAsync(cancellationToken))
        {
            lines.Add(new VoucherLine(
                VoucherLineId: linesReader.GetInt64(0),
                VoucherId: linesReader.GetInt64(1),
                AccountId: linesReader.GetInt64(2),
                AccountCode: linesReader.GetString(3),
                AccountName: linesReader.GetString(4),
                DebitAmount: linesReader.GetDecimal(5),
                CreditAmount: linesReader.GetDecimal(6),
                LineNarration: linesReader.IsDBNull(7) ? null : linesReader.GetString(7)));
        }

        return new Voucher(
            header.VoucherId,
            header.VoucherNo,
            header.VoucherType,
            header.VoucherDate,
            header.Status,
            header.Narration,
            header.SourceType,
            header.SourceId,
            header.PostedAtUtc,
            lines);
    }

    public async Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                la.AccountId,
                la.AccountCode,
                la.AccountName,
                ISNULL(SUM(CASE WHEN v.VoucherId IS NOT NULL THEN vl.DebitAmount ELSE 0 END), 0) AS TotalDebit,
                ISNULL(SUM(CASE WHEN v.VoucherId IS NOT NULL THEN vl.CreditAmount ELSE 0 END), 0) AS TotalCredit
            FROM dbo.LedgerAccounts la
            LEFT JOIN dbo.VoucherLines vl ON vl.AccountId = la.AccountId
            LEFT JOIN dbo.Vouchers v ON v.VoucherId = vl.VoucherId
                AND v.Status = 'Posted'
                AND v.VoucherDate BETWEEN @FromDate AND @ToDate
            WHERE la.IsActive = 1
            GROUP BY la.AccountId, la.AccountCode, la.AccountName
            HAVING
                ISNULL(SUM(CASE WHEN v.VoucherId IS NOT NULL THEN vl.DebitAmount ELSE 0 END), 0) <> 0
                OR
                ISNULL(SUM(CASE WHEN v.VoucherId IS NOT NULL THEN vl.CreditAmount ELSE 0 END), 0) <> 0
            ORDER BY la.AccountCode;
            """;

        var results = new List<TrialBalanceRow>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FromDate", fromDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@ToDate", toDate.ToDateTime(TimeOnly.MinValue));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var totalDebit = reader.GetDecimal(3);
            var totalCredit = reader.GetDecimal(4);
            results.Add(new TrialBalanceRow(
                AccountId: reader.GetInt64(0),
                AccountCode: reader.GetString(1),
                AccountName: reader.GetString(2),
                TotalDebit: totalDebit,
                TotalCredit: totalCredit,
                NetBalance: totalDebit - totalCredit));
        }

        return results;
    }

    public async Task<IReadOnlyList<LedgerEntryRow>> GetLedgerAsync(
        long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT v.PostedAtUtc, v.VoucherNo, v.VoucherType, v.VoucherId, v.Narration, vl.DebitAmount, vl.CreditAmount
            FROM dbo.VoucherLines vl
            INNER JOIN dbo.Vouchers v ON v.VoucherId = vl.VoucherId
            WHERE vl.AccountId = @AccountId
              AND v.Status = 'Posted'
              AND v.VoucherDate BETWEEN @FromDate AND @ToDate
            ORDER BY v.VoucherDate, v.VoucherId, vl.VoucherLineId;
            """;

        var results = new List<LedgerEntryRow>();

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@FromDate", fromDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@ToDate", toDate.ToDateTime(TimeOnly.MinValue));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new LedgerEntryRow(
                PostedAtUtc: reader.GetDateTime(0),
                VoucherNo: reader.GetString(1),
                VoucherType: reader.GetString(2),
                VoucherId: reader.GetInt64(3),
                Narration: reader.IsDBNull(4) ? null : reader.GetString(4),
                DebitAmount: reader.GetDecimal(5),
                CreditAmount: reader.GetDecimal(6)));
        }

        return results;
    }

    private static async Task<long> InsertVoucherAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string voucherNo,
        string voucherType,
        DateOnly voucherDate,
        string? narration,
        string sourceType,
        long sourceId,
        int createdBy,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Vouchers
                (VoucherNo, VoucherType, VoucherDate, Status, Narration, SourceType, SourceId, CreatedBy)
            VALUES
                (@VoucherNo, @VoucherType, @VoucherDate, 'Posted', @Narration, @SourceType, @SourceId, @CreatedBy);

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@VoucherNo", voucherNo);
        command.Parameters.AddWithValue("@VoucherType", voucherType);
        command.Parameters.AddWithValue("@VoucherDate", voucherDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@Narration", (object?)narration ?? DBNull.Value);
        command.Parameters.AddWithValue("@SourceType", sourceType);
        command.Parameters.AddWithValue("@SourceId", sourceId);
        command.Parameters.AddWithValue("@CreatedBy", createdBy <= 0 ? DBNull.Value : createdBy);

        var id = (long?)await command.ExecuteScalarAsync(cancellationToken);
        if (id is null or <= 0)
        {
            throw new InvalidOperationException("Failed to create voucher.");
        }

        return id.Value;
    }

    private static async Task InsertVoucherLineAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long voucherId,
        long accountId,
        decimal debitAmount,
        decimal creditAmount,
        string? lineNarration,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.VoucherLines
                (VoucherId, AccountId, DebitAmount, CreditAmount, LineNarration)
            VALUES
                (@VoucherId, @AccountId, @DebitAmount, @CreditAmount, @LineNarration);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@VoucherId", voucherId);
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@DebitAmount", debitAmount);
        command.Parameters.AddWithValue("@CreditAmount", creditAmount);
        command.Parameters.AddWithValue("@LineNarration", (object?)lineNarration ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string> GetNextDocumentNumberAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string sequenceKey,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.DocumentSequences WITH (UPDLOCK, ROWLOCK)
            SET CurrentValue = CurrentValue + 1,
                UpdatedAt = SYSUTCDATETIME()
            OUTPUT INSERTED.Prefix, INSERTED.CurrentValue, INSERTED.PaddingLength
            WHERE SequenceKey = @SequenceKey;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@SequenceKey", sequenceKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Document sequence '{sequenceKey}' is missing.");
        }

        var prefix = reader.GetString(0);
        var currentValue = reader.GetInt64(1);
        var paddingLength = reader.GetInt32(2);
        await reader.CloseAsync();

        return $"{prefix}{currentValue.ToString().PadLeft(paddingLength, '0')}";
    }

    private static async Task<string> GetVoucherNoAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long voucherId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT VoucherNo
            FROM dbo.Vouchers
            WHERE VoucherId = @VoucherId;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@VoucherId", voucherId);
        var value = (string?)await command.ExecuteScalarAsync(cancellationToken);
        return value ?? string.Empty;
    }

    private static async Task<IReadOnlyList<JournalEntryLine>> GetJournalLinesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long journalId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT AccountId, DebitAmount, CreditAmount, LineNarration
            FROM dbo.JournalEntryLines
            WHERE JournalId = @JournalId
            ORDER BY JournalLineId;
            """;

        var lines = new List<JournalEntryLine>();
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@JournalId", journalId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lines.Add(new JournalEntryLine(
                AccountId: reader.GetInt64(0),
                DebitAmount: reader.GetDecimal(1),
                CreditAmount: reader.GetDecimal(2),
                LineNarration: reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return lines;
    }
}
