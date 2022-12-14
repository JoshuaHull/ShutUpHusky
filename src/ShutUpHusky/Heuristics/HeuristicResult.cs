namespace ShutUpHusky.Heuristics;

internal record HeuristicResult {
    public required double Priority { get; init; }
    public required string Value { get; init; } = string.Empty;
    public string? After { get; init; } = null;

    public static HeuristicResult Default => new() {
        Priority = Constants.NotAPriority,
        Value = string.Empty,
    };
}
