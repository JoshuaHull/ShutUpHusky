using LibGit2Sharp;

namespace ShutUpHusky.Analysis.GitTools;

internal class PatchParser {
    public IEnumerable<ChangedLine> Parse(Patch patch) {
        var content = patch.Content;
        var lines = content.Split('\n');

        /// diff --git a/src/modified.ts b/src/modified.ts
        /// index 811984f..b4d43d5 100644
        /// --- a/src/modified.ts
        /// +++ b/src/modified.ts
        if (lines.Length <= 4)
            yield break;

        var diffLines = lines[4..];

        for (int i = 0; i < diffLines.Length; i += 1) {
            var line = NormalizeLine(diffLines[i]);
            var nextLine = i + 1 >= diffLines.Length
                ? null
                : NormalizeLine(diffLines[i + 1]);

            var changeType = GetChangeType(line, nextLine);

            if (changeType is null)
                continue;

            if (changeType is ChangeType.Added) {
                yield return new() {
                    Type = ChangeType.Added,
                    Content = line[1..],
                    PreviousContent = string.Empty,
                };
                continue;
            }

            if (changeType is ChangeType.Deleted) {
                yield return new() {
                    Type = ChangeType.Deleted,
                    Content = string.Empty,
                    PreviousContent = line[1..],
                };
                continue;
            }

            if (changeType is ChangeType.Replaced) {
                i += 1;
                yield return new() {
                    Type = ChangeType.Replaced,
                    Content = nextLine![1..], // null forgive: a 'Replaced' change type must mean the next line exists and has a lengh >= 1
                    PreviousContent = line[1..],
                };
            }
        }
    }

    private string NormalizeLine(string line) =>
        line.StartsWith("@@") ? line.Split("@@")[2] : line;

    private ChangeType? GetChangeType(string line) =>
        line.Length <= 1
            ? null
            : line[0] switch {
                '+' => ChangeType.Added,
                '-' => ChangeType.Deleted,
                _ => null,
            };

    private ChangeType? GetChangeType(string line, string? nextLine) {
        var lineChangeType = GetChangeType(line);

        if (lineChangeType is null)
            return null;

        if (nextLine is null)
            return lineChangeType;

        var nextLineChangeType = GetChangeType(nextLine);

        if (nextLineChangeType is null)
            return lineChangeType;
    
        if (lineChangeType is ChangeType.Deleted && nextLineChangeType is ChangeType.Added)
            return ChangeType.Replaced;

        return lineChangeType;
    }
}
