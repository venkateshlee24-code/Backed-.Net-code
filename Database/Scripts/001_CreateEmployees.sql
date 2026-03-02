IF OBJECT_ID('dbo.Employees', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeCode NVARCHAR(30) NOT NULL,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        DepartmentCode NVARCHAR(30) NOT NULL,
        JoiningDate DATE NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
        CreatedAt DATETIME2(3) NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_Employees_EmployeeCode ON dbo.Employees(EmployeeCode);
    CREATE UNIQUE INDEX UX_Employees_Email ON dbo.Employees(Email);
    CREATE INDEX IX_Employees_DepartmentCode_IsActive ON dbo.Employees(DepartmentCode, IsActive);
END
