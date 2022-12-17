namespace ShutUpHusky.Heuristics;

internal record HeuristicResult {
    public double Priority { get; init; }
    public required string Value { get; init; } = string.Empty;
    public HeuristicResult? LowerPriorityResult { get; init; }

    public static HeuristicResult AddLowerPriorityResult(HeuristicResult parent, HeuristicResult lowerPriorityResult) =>
        parent with {
            LowerPriorityResult = parent.LowerPriorityResult is null ? lowerPriorityResult : AddLowerPriorityResult(parent.LowerPriorityResult, lowerPriorityResult),
        };

    public static HeuristicResult PrefixAll(HeuristicResult parent, string prefix) =>
        parent with {
            Value = $"{prefix}{parent.Value}",
            LowerPriorityResult = parent.LowerPriorityResult is null ? null : PrefixAll(parent.LowerPriorityResult, prefix),
        };
}
