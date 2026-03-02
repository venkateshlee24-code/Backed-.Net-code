IF OBJECT_ID('dbo.DocumentSequences', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentSequences
    (
        SequenceKey NVARCHAR(30) NOT NULL PRIMARY KEY,
        Prefix NVARCHAR(10) NOT NULL,
        CurrentValue BIGINT NOT NULL,
        PaddingLength INT NOT NULL CONSTRAINT DF_DocumentSequences_PaddingLength DEFAULT (6),
        UpdatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_DocumentSequences_UpdatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.DocumentSequences WHERE SequenceKey = 'SI')
    INSERT INTO dbo.DocumentSequences (SequenceKey, Prefix, CurrentValue, PaddingLength)
    VALUES ('SI', 'SI-', 0, 6);

IF NOT EXISTS (SELECT 1 FROM dbo.DocumentSequences WHERE SequenceKey = 'PI')
    INSERT INTO dbo.DocumentSequences (SequenceKey, Prefix, CurrentValue, PaddingLength)
    VALUES ('PI', 'PI-', 0, 6);

IF NOT EXISTS (SELECT 1 FROM dbo.DocumentSequences WHERE SequenceKey = 'PAY')
    INSERT INTO dbo.DocumentSequences (SequenceKey, Prefix, CurrentValue, PaddingLength)
    VALUES ('PAY', 'PAY-', 0, 6);

IF NOT EXISTS (SELECT 1 FROM dbo.DocumentSequences WHERE SequenceKey = 'JRN')
    INSERT INTO dbo.DocumentSequences (SequenceKey, Prefix, CurrentValue, PaddingLength)
    VALUES ('JRN', 'JRN-', 0, 6);

IF NOT EXISTS (SELECT 1 FROM dbo.DocumentSequences WHERE SequenceKey = 'VCH')
    INSERT INTO dbo.DocumentSequences (SequenceKey, Prefix, CurrentValue, PaddingLength)
    VALUES ('VCH', 'VCH-', 0, 6);
GO
