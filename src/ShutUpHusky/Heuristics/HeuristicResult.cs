namespace ShutUpHusky.Heuristics;

internal record HeuristicResult {
    public required int Priority { get; init; }
    public required string Value { get; init; } = string.Empty;
    public string? After { get; init; } = null;
}
