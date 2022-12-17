namespace ShutUpHusky.Heuristics;

internal record HeuristicResult {
    public double Priority { get; init; }
    public required string Value { get; init; } = string.Empty;
    public HeuristicResult? Shortened { get; init; }

    public static HeuristicResult AddShortened(HeuristicResult parent, HeuristicResult shortened) =>
        parent with {
            Shortened = parent.Shortened is null ? shortened : AddShortened(parent.Shortened, shortened),
        };

    public static HeuristicResult PrefixAll(HeuristicResult parent, string prefix) =>
        parent with {
            Value = $"{prefix}{parent.Value}",
            Shortened = parent.Shortened is null ? null : PrefixAll(parent.Shortened, prefix),
        };
}
