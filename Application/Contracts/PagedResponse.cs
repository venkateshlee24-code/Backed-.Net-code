namespace MyWebApi.Application.Contracts;

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    IReadOnlyList<T> Items
);
