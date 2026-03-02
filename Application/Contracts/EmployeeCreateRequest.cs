namespace MyWebApi.Application.Contracts;

public sealed record EmployeeCreateRequest(
    string EmployeeCode,
    string FullName,
    string Email,
    string DepartmentCode,
    DateOnly JoiningDate,
    string Password
);
