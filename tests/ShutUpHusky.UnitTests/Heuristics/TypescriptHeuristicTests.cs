using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Utils;

namespace ShutUpHusky.UnitTests.Heuristics;

public class TypescriptHeuristicTests
{
    private TypescriptHeuristic Heuristic => new();

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

        var repo = new MockRepository {
            Head = new MockBranch {
                Tip = new MockCommit {
                    Tree = new MockTree {
                    }.Object,
                }.Object,
            }.Object,
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "src/modifiedFile.ts",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("src/modifiedFile.ts", modifiedTypescriptFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new HeuristicResult[] {
            new() {
                Priority = 0.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 3),
                Value = "replaced secondReplacedLine value 11 with unrelatedConstant [1, 2, 3]",
                After = ", ",
            },
            new() {
                Priority = 1.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 3),
                Value = "replaced wow with wew",
                After = ", ",
            },
            new() {
                Priority = 2.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 3),
                Value = "added const wowStatement = wowow",
                After = ", ",
            },
        }).And.BeInDescendingOrder(r => r.Priority);
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

        var repo = new MockRepository {
            Head = new MockBranch {
                Tip = new MockCommit {
                    Tree = new MockTree {
                    }.Object,
                }.Object,
            }.Object,
            Status = new MockRepositoryStatus {
                StatusEntries = new[] {
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "src/CommitMessage.ts",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("src/CommitMessage.ts", newTypescriptFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new HeuristicResult[] {
            new() {
                Priority = 0.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added evaluate => double = => value length / 3 00",
                After = ", ",
            },
            new() {
                Priority = 1.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added addSnippet snippet string",
                After = ", ",
            },
            new() {
                Priority = 2.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added value string",
                After = ", ",
            },
            new() {
                Priority = 3.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added const trimmed = snippet trim",
                After = ", ",
            },
            new() {
                Priority = 4.ToPriority(Constants.LowPriorty, Constants.LanguageSpecificPriority, 5),
                Value = "added class CommitMessage implements ICommitMessage",
                After = ", ",
            },
        }).And.BeInDescendingOrder(r => r.Priority);
    }
}
