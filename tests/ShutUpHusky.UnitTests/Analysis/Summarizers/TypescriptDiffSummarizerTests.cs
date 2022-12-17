using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Analysis.Summarizers;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.UnitTests.Analysis.Summarizers;

public class TypescriptDiffSummarizerTests
{
    [Test]
    public void ShouldWriteAReasonableCommitMessage_FromReplacedLines() {
        // Arrange
        var modifiedTypescriptFile = new MockPatch {
            Content =
                """
                diff --git a/src/modified.ts b/src/modified.ts
                index 811984f..b4d43d5 100644
                --- a/src/modified.ts
                +++ b/src/modified.ts
                @@ -3,5 +3,7 @@ import { stuff } from './place';
                 const modifiedFile = 'new file';
                 
                 function makeChange() {
                -    const wewStatement = 'wow';
                +    const wewStatement = 'wew';
                +
                +    const wowStatement = 'wowow';
                -    const secondReplacedLine = { value: 11 }
                +    const unrelatedConstant = [1, 2, 3]
                 }
                """,
        };

        var modifiedTypescriptFileStatusEntry = new MockStatusEntry {
            FilePath = "src/modifiedFile.cs",
            State = FileStatus.ModifiedInIndex,
        };

        // Act
        var result = SummarizerFactory
            .Create(modifiedTypescriptFileStatusEntry.Object)
            ?.Summarize(modifiedTypescriptFile.Object) ?? throw new Exception(
                $"No summarizer found for test {nameof(ShouldWriteAReasonableCommitMessage_FromReplacedLines)}"
            );

        // Assert
        result.Should().BeEquivalentTo(
            new HeuristicResult {
                Value = "replaced secondReplacedLine value 11 with unrelatedConstant [1, 2, 3]",
                LowerPriorityResult = new() {
                    Value = "replaced wow with wew",
                    LowerPriorityResult = new() {
                        Value = "added const wowStatement = wowow",
                    },
                },
            }
        );
    }

    [Test]
    public void ShouldWriteMeaningfulCommitMessage_FromAddedProperties_AndFunctions_AndClasses_Etc() {
        // Arrange
        var newTypescriptFile = new MockPatch {
            Content =
                """
                diff --git a/src/CommitMessage.ts b/src/CommitMessage.ts
                index 811984f..b4d43d5 100644
                --- /dev/null
                +++ b/src/CommitMessage.ts
                @@ -3,5 +3,7 @@+import { git } from './command-line';
                +public class CommitMessage implements ICommitMessage {
                +
                +    public value: string;
                +
                +    public addSnippet(snippet: string): void {
                +        const trimmed = snippet.trim();
                +    }
                +
                +    evaluate: () => double = () => value.length / 3.00;
                +}
                """,
        };

        var newTypescriptFileStatusEntry = new MockStatusEntry {
            FilePath = "src/CommitMessage.ts",
            State = FileStatus.NewInIndex,
        };

        // Act
        var result = SummarizerFactory
            .Create(newTypescriptFileStatusEntry.Object)
            ?.Summarize(newTypescriptFile.Object) ?? throw new Exception(
                $"No summarizer found for test {nameof(ShouldWriteMeaningfulCommitMessage_FromAddedProperties_AndFunctions_AndClasses_Etc)}"
            );

        // Assert
        result.Should().BeEquivalentTo(
            new HeuristicResult {
                Value = "added evaluate => double = => value length / 3 00",
                LowerPriorityResult = new() {
                    Value = "added addSnippet snippet string",
                    LowerPriorityResult = new() {
                        Value = "added value string",
                        LowerPriorityResult = new() {
                            Value = "added const trimmed = snippet trim",
                            LowerPriorityResult = new() {
                                Value = "added class CommitMessage implements ICommitMessage",
                            },
                        },
                    },
                },
            }
        );
    }
}
