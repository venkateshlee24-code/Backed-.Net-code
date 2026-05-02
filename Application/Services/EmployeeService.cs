using MyWebApi.Application.Contracts;
using MyWebApi.Domain.Models;
using MyWebApi.Infrastructure.Repositories;

namespace MyWebApi.Application.Services;

public sealed class EmployeeService(IEmployeeRepository repository) : IEmployeeService
{
    public async Task<PagedResponse<Employee>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize switch
        {
            < 1 => 20,
            > 200 => 200,
            _ => pageSize
        };

        var items = await repository.GetPagedAsync(safePage, safePageSize, cancellationToken);
        return new PagedResponse<Employee>(safePage, safePageSize, items);
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Task.FromResult<Employee?>(null);
        }

        return repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<int> CreateAsync(EmployeeCreateRequest request, CancellationToken cancellationToken)
    {
        ValidateForCreate(request);

        var emailExists = await repository.EmailExistsAsync(request.Email.Trim(), null, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Employee email already exists.");
        }

        return await repository.CreateAsync(request, cancellationToken);
    }

    public async Task<bool> UpdateAsync(int id, EmployeeUpdateRequest request, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return false;
        }

        ValidateForUpdate(request);

        var emailExists = await repository.EmailExistsAsync(request.Email.Trim(), id, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Employee email already exists.");
        }

        return await repository.UpdateAsync(id, request, cancellationToken);
    }

    public Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Task.FromResult(false);
        }

        return repository.DeactivateAsync(id, cancellationToken);
    }

    private static void ValidateForCreate(EmployeeCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeCode) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            throw new ArgumentException("Employee code, full name, email, and department code are required.");
        }
    }

    private static void ValidateForUpdate(EmployeeUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            throw new ArgumentException("Full name, email, and department code are required.");
        }
    }

    public async Task<(int TotalCount, IReadOnlyList<User> Data)> GetUsersAsync(
    int page,
    int pageSize,
    string search,
    CancellationToken cancellationToken)
    {
        return await repository.GetUsersAsync(page, pageSize, search, cancellationToken);
    }
}
