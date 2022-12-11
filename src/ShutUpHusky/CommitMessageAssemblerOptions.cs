namespace ShutUpHusky;

public record CommitMessageAssemblerOptions {
    public bool EnableBody { get; init; }
    public bool EnableExperimentalHeuristics { get; init; }
}
