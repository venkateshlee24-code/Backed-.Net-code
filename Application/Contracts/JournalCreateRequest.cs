public sealed class JournalCreateRequest
{
    public DateOnly JournalDate { get; init; }
    public string? Narration { get; init; }
    public IReadOnlyList<JournalLineRequest> Lines { get; init; } = [];
}
