namespace ShutUpHusky.Analysis.Tokenizing;

public record TokenizerOptions {
    public required string SplitRegex { get; init; }
}
