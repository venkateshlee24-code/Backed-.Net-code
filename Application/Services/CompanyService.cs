using MyWebApi.Application.Contracts;
using MyWebApi.Application.Services;



namespace MyWebApi.Application.Services;

public sealed class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repo;

    public CompanyService(ICompanyRepository repo)
    {
        _repo = repo;
    }

    public async Task<long> CreateAsync(
        CompanyCreateRequest request,
        CancellationToken cancellationToken)
    {
        return await _repo.CreateAsync(request, cancellationToken);
    }
    public async Task<IReadOnlyList<Company>> GetAllAsync(
    CancellationToken cancellationToken)
{
    return await _repo.GetAllAsync(cancellationToken);
}

public async Task<Company?> GetByIdAsync(
    long id,
    CancellationToken cancellationToken)
{
    return await _repo.GetByIdAsync(id, cancellationToken);
}

public async Task<bool> UpdateAsync(
    long id,
    CompanyCreateRequest request,
    CancellationToken cancellationToken)
{
    return await _repo.UpdateAsync(id, request, cancellationToken);
}

public async Task<bool> DeactivateAsync(
    long id,
    CancellationToken cancellationToken)
{
    return await _repo.DeactivateAsync(id, cancellationToken);
}

}
