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
        });
    }
}
