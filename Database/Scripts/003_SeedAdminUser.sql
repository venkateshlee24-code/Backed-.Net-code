DECLARE @Email NVARCHAR(150) = 'admin@company.com';
DECLARE @UserCode NVARCHAR(30) = 'USR001';
DECLARE @UserName NVARCHAR(120) = 'Admin User';
DECLARE @PasswordHash NVARCHAR(500) = '$2a$11$FyPH5L769Ow5fXpQAeYzMuiwGCueFMz.0m6ojMgbjKiwWLF/vddnO'; -- Admin@123

DECLARE @UserId INT;
DECLARE @InitiatorRoleId INT;
DECLARE @AuthoriserRoleId INT;
DECLARE @ApModuleId INT;
DECLARE @ArModuleId INT;
DECLARE @GlModuleId INT;

SELECT @InitiatorRoleId = Id FROM dbo.Roles WHERE RoleCode = 'INITIATOR';
SELECT @AuthoriserRoleId = Id FROM dbo.Roles WHERE RoleCode = 'AUTHORISER';
SELECT @ApModuleId = Id FROM dbo.Modules WHERE ModuleCode = 'AP';
SELECT @ArModuleId = Id FROM dbo.Modules WHERE ModuleCode = 'AR';
IF NOT EXISTS (SELECT 1 FROM dbo.Modules WHERE ModuleCode = 'GL')
BEGIN
    INSERT INTO dbo.Modules (ModuleCode, ModuleName) VALUES ('GL', 'General Ledger');
END;

SELECT @GlModuleId = Id FROM dbo.Modules WHERE ModuleCode = 'GL';

IF @InitiatorRoleId IS NULL OR @AuthoriserRoleId IS NULL OR @ApModuleId IS NULL OR @ArModuleId IS NULL OR @GlModuleId IS NULL
BEGIN
    THROW 51000, 'Seed prerequisites missing. Ensure roles/modules are seeded first.', 1;
END;

IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
BEGIN
    UPDATE dbo.Users
    SET UserCode = @UserCode,
        UserName = @UserName,
        PasswordHash = @PasswordHash,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Email = @Email;
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserCode, UserName, Email, PasswordHash, IsActive)
    VALUES (@UserCode, @UserName, @Email, @PasswordHash, 1);
END;

SELECT @UserId = Id FROM dbo.Users WHERE Email = @Email;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoleAssignments
    WHERE UserId = @UserId AND RoleId = @InitiatorRoleId AND ModuleId = @ApModuleId
)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId, ModuleId, IsActive)
    VALUES (@UserId, @InitiatorRoleId, @ApModuleId, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoleAssignments
    WHERE UserId = @UserId AND RoleId = @AuthoriserRoleId AND ModuleId = @ApModuleId
)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId, ModuleId, IsActive)
    VALUES (@UserId, @AuthoriserRoleId, @ApModuleId, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoleAssignments
    WHERE UserId = @UserId AND RoleId = @InitiatorRoleId AND ModuleId = @ArModuleId
)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId, ModuleId, IsActive)
    VALUES (@UserId, @InitiatorRoleId, @ArModuleId, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoleAssignments
    WHERE UserId = @UserId AND RoleId = @AuthoriserRoleId AND ModuleId = @ArModuleId
)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId, ModuleId, IsActive)
    VALUES (@UserId, @AuthoriserRoleId, @ArModuleId, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoleAssignments
    WHERE UserId = @UserId AND RoleId = @InitiatorRoleId AND ModuleId = @GlModuleId
)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId, ModuleId, IsActive)
    VALUES (@UserId, @InitiatorRoleId, @GlModuleId, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoleAssignments
    WHERE UserId = @UserId AND RoleId = @AuthoriserRoleId AND ModuleId = @GlModuleId
)
BEGIN
    INSERT INTO dbo.UserRoleAssignments (UserId, RoleId, ModuleId, IsActive)
    VALUES (@UserId, @AuthoriserRoleId, @GlModuleId, 1);
END;
