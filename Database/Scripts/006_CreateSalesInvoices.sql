IF OBJECT_ID('dbo.SalesInvoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesInvoices
    (
        InvoiceId BIGINT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNo NVARCHAR(30) NOT NULL,
        InvoiceDate DATE NOT NULL,
        CustomerName NVARCHAR(150) NOT NULL,
        ReceivableAccountId BIGINT NOT NULL,
        RevenueAccountId BIGINT NOT NULL,
        TaxAccountId BIGINT NULL,
        SubTotal DECIMAL(18,2) NOT NULL,
        TaxTotal DECIMAL(18,2) NOT NULL,
        GrandTotal DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(15) NOT NULL CONSTRAINT DF_SalesInvoices_Status DEFAULT ('Draft'),
        Narration NVARCHAR(500) NULL,
        VoucherId BIGINT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_SalesInvoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2(3) NULL,
        UpdatedBy INT NULL,
        CONSTRAINT FK_SalesInvoices_ReceivableAccount FOREIGN KEY (ReceivableAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_SalesInvoices_RevenueAccount FOREIGN KEY (RevenueAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_SalesInvoices_TaxAccount FOREIGN KEY (TaxAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_SalesInvoices_Vouchers FOREIGN KEY (VoucherId) REFERENCES dbo.Vouchers(VoucherId)
    );

    CREATE UNIQUE INDEX UX_SalesInvoices_InvoiceNo ON dbo.SalesInvoices(InvoiceNo);
    CREATE INDEX IX_SalesInvoices_Status_InvoiceDate ON dbo.SalesInvoices(Status, InvoiceDate);
END
GO
