namespace ShutUpHusky.Heuristics;

internal record HeuristicResult {
    public int Priority { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? After { get; init; } = null;
}
