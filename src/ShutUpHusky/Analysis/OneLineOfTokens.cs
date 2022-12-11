namespace ShutUpHusky.Analysis;

internal record OneLineOfTokens {
    public required ChangeType ChangeType { get; init; }
    public required string[] Tokens { get; init; }
    public required string[] BeforeTokens { get; init; }
    public required string[] AfterTokens { get; init; }

    public override string ToString() =>
        ChangeType switch {
            ChangeType.Added => $"added {string.Join(" ", Tokens)}",
            ChangeType.Deleted => $"deleted {string.Join(" ", Tokens)}",
            ChangeType.Replaced => ToReplacedString(),
            _ => string.Join(" ", Tokens),
        };

    private string ToReplacedString() =>
        Tokens.Length switch {
            0 => string.Empty,
            1 => $"changed {Tokens[0]}",
            _ => $"replaced {string.Join(" ", BeforeTokens)} with {string.Join(" ", AfterTokens)}",
        };
}
