public sealed record Company(
    long CompanyId,
    string CompanyCode,
    string CompanyName,
    int BaseCurrencyId,
    bool IsActive
);
