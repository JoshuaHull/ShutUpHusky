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
        (Tokens.Length, BeforeTokens.Length, AfterTokens.Length) switch {
            (0, _, _) => string.Empty,
            (1, _, _) => $"changed {Tokens[0]}",
            (_, 0, 0) => $"changed {string.Join(" ", Tokens)}",
            (_, 0, _) => $"added {string.Join(" ", AfterTokens)}",
            (_, _, 0) => $"removed {string.Join(" ", BeforeTokens)}",
            (_, _, _) => $"replaced {string.Join(" ", BeforeTokens)} with {string.Join(" ", AfterTokens)}",
        };
}
