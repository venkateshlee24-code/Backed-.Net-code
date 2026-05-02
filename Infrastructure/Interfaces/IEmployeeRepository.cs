using MyWebApi.Application.Contracts;
using MyWebApi.Domain.Models;

namespace MyWebApi.Infrastructure.Repositories;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<int> CreateAsync(EmployeeCreateRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, EmployeeUpdateRequest request, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId, CancellationToken cancellationToken);

  Task<(int TotalCount, IReadOnlyList<User> Data)> GetUsersAsync(
    int page,
    int pageSize,
    string search,
    CancellationToken cancellationToken);
}
