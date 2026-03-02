namespace MyWebApi.Domain.Models;

public sealed record AuthUser(
    int Id,
    string Email,
    string UserName,
    string PasswordHash,
    bool IsActive
);
