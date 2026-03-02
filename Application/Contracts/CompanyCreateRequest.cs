public sealed class CompanyCreateRequest
{
    public string CompanyCode { get; init; } = default!;
    public string CompanyName { get; init; } = default!;
    public int BaseCurrencyId { get; init; }
    public long CreatedBy { get; init; }
}
