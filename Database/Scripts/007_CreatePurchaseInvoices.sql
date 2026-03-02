IF OBJECT_ID('dbo.PurchaseInvoices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseInvoices
    (
        InvoiceId BIGINT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNo NVARCHAR(30) NOT NULL,
        InvoiceDate DATE NOT NULL,
        VendorName NVARCHAR(150) NOT NULL,
        PayableAccountId BIGINT NOT NULL,
        ExpenseAccountId BIGINT NOT NULL,
        TaxAccountId BIGINT NULL,
        SubTotal DECIMAL(18,2) NOT NULL,
        TaxTotal DECIMAL(18,2) NOT NULL,
        GrandTotal DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(15) NOT NULL CONSTRAINT DF_PurchaseInvoices_Status DEFAULT ('Draft'),
        Narration NVARCHAR(500) NULL,
        VoucherId BIGINT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_PurchaseInvoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2(3) NULL,
        UpdatedBy INT NULL,
        CONSTRAINT FK_PurchaseInvoices_PayableAccount FOREIGN KEY (PayableAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_PurchaseInvoices_ExpenseAccount FOREIGN KEY (ExpenseAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_PurchaseInvoices_TaxAccount FOREIGN KEY (TaxAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_PurchaseInvoices_Vouchers FOREIGN KEY (VoucherId) REFERENCES dbo.Vouchers(VoucherId)
    );

    CREATE UNIQUE INDEX UX_PurchaseInvoices_InvoiceNo ON dbo.PurchaseInvoices(InvoiceNo);
    CREATE INDEX IX_PurchaseInvoices_Status_InvoiceDate ON dbo.PurchaseInvoices(Status, InvoiceDate);
END
GO
