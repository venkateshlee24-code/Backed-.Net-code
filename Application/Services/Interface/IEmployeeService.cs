using MyWebApi.Application.Contracts;
using MyWebApi.Domain.Models;

namespace MyWebApi.Application.Services;

public interface IEmployeeService
{
    Task<PagedResponse<Employee>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<int> CreateAsync(EmployeeCreateRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, EmployeeUpdateRequest request, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken);

    Task<(int TotalCount, IReadOnlyList<User> Data)> GetUsersAsync(
    int page,
    int pageSize,
    string search,
    CancellationToken cancellationToken);
}
