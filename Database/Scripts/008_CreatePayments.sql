IF OBJECT_ID('dbo.Payments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        PaymentId BIGINT IDENTITY(1,1) PRIMARY KEY,
        PaymentNo NVARCHAR(30) NOT NULL,
        PaymentDate DATE NOT NULL,
        PartyType NVARCHAR(20) NOT NULL,
        PartyName NVARCHAR(150) NOT NULL,
        PaymentType NVARCHAR(20) NOT NULL,
        OffsetAccountId BIGINT NOT NULL,
        CashBankAccountId BIGINT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(15) NOT NULL CONSTRAINT DF_Payments_Status DEFAULT ('Draft'),
        Narration NVARCHAR(500) NULL,
        VoucherId BIGINT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2(3) NULL,
        UpdatedBy INT NULL,
        CONSTRAINT FK_Payments_OffsetAccount FOREIGN KEY (OffsetAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_Payments_CashBankAccount FOREIGN KEY (CashBankAccountId) REFERENCES dbo.LedgerAccounts(AccountId),
        CONSTRAINT FK_Payments_Vouchers FOREIGN KEY (VoucherId) REFERENCES dbo.Vouchers(VoucherId)
    );

    CREATE UNIQUE INDEX UX_Payments_PaymentNo ON dbo.Payments(PaymentNo);
    CREATE INDEX IX_Payments_Status_PaymentDate ON dbo.Payments(Status, PaymentDate);
END
GO

IF OBJECT_ID('dbo.JournalEntries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalEntries
    (
        JournalId BIGINT IDENTITY(1,1) PRIMARY KEY,
        JournalNo NVARCHAR(30) NOT NULL,
        JournalDate DATE NOT NULL,
        Status NVARCHAR(15) NOT NULL CONSTRAINT DF_JournalEntries_Status DEFAULT ('Draft'),
        Narration NVARCHAR(500) NULL,
        VoucherId BIGINT NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_JournalEntries_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2(3) NULL,
        UpdatedBy INT NULL,
        CONSTRAINT FK_JournalEntries_Vouchers FOREIGN KEY (VoucherId) REFERENCES dbo.Vouchers(VoucherId)
    );

    CREATE UNIQUE INDEX UX_JournalEntries_JournalNo ON dbo.JournalEntries(JournalNo);
    CREATE INDEX IX_JournalEntries_Status_JournalDate ON dbo.JournalEntries(Status, JournalDate);
END
GO

IF OBJECT_ID('dbo.JournalEntryLines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.JournalEntryLines
    (
        JournalLineId BIGINT IDENTITY(1,1) PRIMARY KEY,
        JournalId BIGINT NOT NULL,
        AccountId BIGINT NOT NULL,
        DebitAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_JournalEntryLines_DebitAmount DEFAULT (0),
        CreditAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_JournalEntryLines_CreditAmount DEFAULT (0),
        LineNarration NVARCHAR(500) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_JournalEntryLines_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_JournalEntryLines_JournalEntries FOREIGN KEY (JournalId) REFERENCES dbo.JournalEntries(JournalId),
        CONSTRAINT FK_JournalEntryLines_LedgerAccounts FOREIGN KEY (AccountId) REFERENCES dbo.LedgerAccounts(AccountId)
    );

    CREATE INDEX IX_JournalEntryLines_JournalId ON dbo.JournalEntryLines(JournalId);
END
GO
