using MyWebApi.Application.Contracts;

namespace MyWebApi.Application.Services;

public interface ICompanyService
{
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken);
    Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<long> CreateAsync(CompanyCreateRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(long id, CompanyCreateRequest request, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken);
}

