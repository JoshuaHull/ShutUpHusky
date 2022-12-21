namespace ShutUpHusky.Heuristics;

internal record HeuristicResult {
    public double Priority { get; init; }
    public required string Value { get; init; } = string.Empty;
    public HeuristicResult? LowerPriorityResult { get; init; }

    public static HeuristicResult WithLowerPriorityResult(HeuristicResult parent, HeuristicResult lowerPriorityResult) =>
        parent with {
            LowerPriorityResult = parent.LowerPriorityResult is null ? lowerPriorityResult : WithLowerPriorityResult(parent.LowerPriorityResult, lowerPriorityResult),
        };

    public static HeuristicResult WithAllPrefixed(HeuristicResult parent, string prefix) =>
        parent with {
            Value = $"{prefix}{parent.Value}",
            LowerPriorityResult = parent.LowerPriorityResult is null ? null : WithAllPrefixed(parent.LowerPriorityResult, prefix),
        };
}
