namespace ShutUpHusky.Heuristics;

public record HeuristicResult {
    public int Priority { get; init; }
    public string Value { get; init; } = string.Empty;
}