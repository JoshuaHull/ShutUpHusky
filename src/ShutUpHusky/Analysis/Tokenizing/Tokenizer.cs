using System.Text.RegularExpressions;

namespace ShutUpHusky.Analysis.Tokenizing;

internal class Tokenizer {
    private readonly TokenizerOptions _options;
    private readonly Regex _splitRegex;

    public Tokenizer(TokenizerOptions options) {
        _options = options;
        _splitRegex = new Regex(_options.SplitRegex);
    }

    public IEnumerable<OneLineOfTokens> Tokenize(IEnumerable<ChangedLine> changedLines) =>
        changedLines.Select(Tokenize);

    public OneLineOfTokens Tokenize(ChangedLine changedLine) {
        var changeType = changedLine.Type;
        var previousContentTokens = Split(changedLine.PreviousContent);
        var currentContentTokens = Split(changedLine.Content);

        if (changeType is ChangeType.Replaced) {
            var beforeTokens = previousContentTokens
                .Except(currentContentTokens)
                .ToArray();

            var afterTokens = currentContentTokens
                .Except(previousContentTokens)
                .ToArray();

            var replacedTokens = beforeTokens
                .Union(afterTokens)
                .ToArray();

            return new() {
                ChangeType = ChangeType.Replaced,
                BeforeTokens = beforeTokens,
                AfterTokens = afterTokens,
                Tokens = replacedTokens,
            };
        }

        return changeType switch {
            ChangeType.Added => new() {
                ChangeType = ChangeType.Added,
                BeforeTokens = Array.Empty<string>(),
                AfterTokens = currentContentTokens,
                Tokens = currentContentTokens,
            },
            ChangeType.Deleted => new() {
                ChangeType = ChangeType.Deleted,
                BeforeTokens = previousContentTokens,
                AfterTokens = Array.Empty<string>(),
                Tokens = previousContentTokens,
            },
            _ => throw new NotImplementedException(),
        };
    }

    private string[] Split(string content) =>
        _splitRegex
            .Split(content)
            .Where(token => token != string.Empty)
            .ToArray();
}
