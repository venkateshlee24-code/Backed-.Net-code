IF OBJECT_ID('dbo.Vouchers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vouchers
    (
        VoucherId BIGINT IDENTITY(1,1) PRIMARY KEY,
        VoucherNo NVARCHAR(30) NOT NULL,
        VoucherType NVARCHAR(10) NOT NULL,
        VoucherDate DATE NOT NULL,
        Status NVARCHAR(15) NOT NULL,
        Narration NVARCHAR(500) NULL,
        SourceType NVARCHAR(30) NULL,
        SourceId BIGINT NULL,
        PostedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_Vouchers_PostedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Vouchers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy INT NULL
    );

    CREATE UNIQUE INDEX UX_Vouchers_VoucherNo ON dbo.Vouchers(VoucherNo);
    CREATE INDEX IX_Vouchers_VoucherDate_Company_Status ON dbo.Vouchers(VoucherDate, Status);
END
GO

IF OBJECT_ID('dbo.VoucherLines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VoucherLines
    (
        VoucherLineId BIGINT IDENTITY(1,1) PRIMARY KEY,
        VoucherId BIGINT NOT NULL,
        AccountId BIGINT NOT NULL,
        DebitAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_VoucherLines_DebitAmount DEFAULT (0),
        CreditAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_VoucherLines_CreditAmount DEFAULT (0),
        LineNarration NVARCHAR(500) NULL,
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_VoucherLines_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_VoucherLines_Vouchers FOREIGN KEY (VoucherId) REFERENCES dbo.Vouchers(VoucherId),
        CONSTRAINT FK_VoucherLines_LedgerAccounts FOREIGN KEY (AccountId) REFERENCES dbo.LedgerAccounts(AccountId)
    );

    CREATE INDEX IX_VoucherLines_VoucherId_AccountId ON dbo.VoucherLines(VoucherId, AccountId);
END
GO
