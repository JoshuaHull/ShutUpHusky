namespace ShutUpHusky.Analysis.Statistics;

public record TokenScoreboardOptions {
    public required string[] IgnoredTokens { get; init; }
    public string[] IgnoreLinesStartingWithToken { get; init; } = Array.Empty<string>();
}
