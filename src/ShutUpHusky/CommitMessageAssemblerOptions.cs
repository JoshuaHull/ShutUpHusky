namespace ShutUpHusky;

public record CommitMessageAssemblerOptions {
    public bool EnableBody { get; init; }
    public bool EnableSummaries { get; init; }
}
