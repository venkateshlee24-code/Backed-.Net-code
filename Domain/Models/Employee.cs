namespace MyWebApi.Domain.Models;

public sealed record Employee(
    int Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string DepartmentCode,
    DateOnly JoiningDate,
    bool IsActive
);
