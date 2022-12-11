namespace ShutUpHusky.Analysis;

internal record ChangedLine {
    public required ChangeType Type { get; init; }
    public required string Content { get; init; }
    public required string PreviousContent { get; init; }

    public override string ToString() =>
        Type switch {
            ChangeType.Added => $"added {Content}",
            ChangeType.Deleted => $"deleted {PreviousContent}",
            ChangeType.Replaced => $"replaced {PreviousContent} with {Content}",
            _ => string.Empty,
        };
}
