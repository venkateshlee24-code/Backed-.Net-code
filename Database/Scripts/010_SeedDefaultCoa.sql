IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '110100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('110100', 'Cash in Hand', 'Asset');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '110200')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('110200', 'Bank Account', 'Asset');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '120100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('120100', 'Accounts Receivable', 'Asset');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '210100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('210100', 'Accounts Payable', 'Liability');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '220100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('220100', 'Output Tax Payable', 'Liability');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '130100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('130100', 'Input Tax Credit', 'Asset');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '410100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('410100', 'Sales Revenue', 'Revenue');

IF NOT EXISTS (SELECT 1 FROM dbo.LedgerAccounts WHERE AccountCode = '510100')
    INSERT INTO dbo.LedgerAccounts (AccountCode, AccountName, AccountType)
    VALUES ('510100', 'Purchase Expense', 'Expense');
GO
