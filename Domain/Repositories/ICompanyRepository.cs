public interface ICompanyRepository
{
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken);
    Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<long> CreateAsync(CompanyCreateRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(long id, CompanyCreateRequest request, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(long id, CancellationToken cancellationToken);
}

