IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserCode NVARCHAR(30) NOT NULL,
        UserName NVARCHAR(120) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(3) NULL
    );

    CREATE UNIQUE INDEX UX_Users_UserCode ON dbo.Users(UserCode);
    CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users(Email);
END
GO

IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleCode NVARCHAR(30) NOT NULL,
        RoleName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT (1),
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_Roles_RoleCode ON dbo.Roles(RoleCode);
END
GO

IF OBJECT_ID('dbo.Modules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Modules
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ModuleCode NVARCHAR(30) NOT NULL,
        ModuleName NVARCHAR(100) NOT NULL
    );

    CREATE UNIQUE INDEX UX_Modules_ModuleCode ON dbo.Modules(ModuleCode);
END
GO

IF OBJECT_ID('dbo.UserRoleAssignments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoleAssignments
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        ModuleId INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_UserRoleAssignments_IsActive DEFAULT (1),
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_UserRoleAssignments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_UserRoleAssignments_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_UserRoleAssignments_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),
        CONSTRAINT FK_UserRoleAssignments_Modules FOREIGN KEY (ModuleId) REFERENCES dbo.Modules(Id)
    );

    CREATE UNIQUE INDEX UX_UserRoleAssignments_UserRoleModule
        ON dbo.UserRoleAssignments(UserId, RoleId, ModuleId);
END
GO

IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TokenHash NVARCHAR(128) NOT NULL,
        ExpiresAtUtc DATETIME2(3) NOT NULL,
        RevokedAtUtc DATETIME2(3) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedByIp NVARCHAR(64) NULL,
        UserAgent NVARCHAR(300) NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );

    CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash ON dbo.RefreshTokens(TokenHash);
    CREATE INDEX IX_RefreshTokens_UserId_RevokedAtUtc_ExpiresAtUtc
        ON dbo.RefreshTokens(UserId, RevokedAtUtc, ExpiresAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleCode = 'INITIATOR')
    INSERT INTO dbo.Roles (RoleCode, RoleName) VALUES ('INITIATOR', 'Initiator');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleCode = 'AUTHORISER')
    INSERT INTO dbo.Roles (RoleCode, RoleName) VALUES ('AUTHORISER', 'Authoriser');

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'AP')
    INSERT INTO dbo.Modules (ModuleCode, ModuleName) VALUES ('AP', 'Accounts Payable');

IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'AR')
    INSERT INTO dbo.Modules (ModuleCode, ModuleName) VALUES ('AR', 'Accounts Receivable');
GO
