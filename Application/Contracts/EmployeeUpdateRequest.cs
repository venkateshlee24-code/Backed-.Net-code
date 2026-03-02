namespace MyWebApi.Application.Contracts;

public sealed record EmployeeUpdateRequest(
    string FullName,
    string Email,
    string DepartmentCode,
    bool IsActive
);
