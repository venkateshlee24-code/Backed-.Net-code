IF OBJECT_ID('dbo.LedgerAccounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LedgerAccounts
    (
        AccountId BIGINT IDENTITY(1,1) PRIMARY KEY,
        AccountCode NVARCHAR(30) NOT NULL,
        AccountName NVARCHAR(150) NOT NULL,
        AccountType NVARCHAR(20) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_LedgerAccounts_IsActive DEFAULT (1),
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_LedgerAccounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(3) NULL
    );

    CREATE UNIQUE INDEX UX_LedgerAccounts_AccountCode ON dbo.LedgerAccounts(AccountCode);
    CREATE INDEX IX_LedgerAccounts_AccountType_IsActive ON dbo.LedgerAccounts(AccountType, IsActive);
END
GO
