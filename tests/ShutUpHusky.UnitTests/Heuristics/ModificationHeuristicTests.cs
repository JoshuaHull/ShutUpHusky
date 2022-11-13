using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests.Heuristics;

public class ModificationHeuristicTests
{
    private ModificationHeuristic Heuristic => new();

    [Test]
    public void ShouldReturnUpdatedLabel_ForTheMostChangedFile()
    {
        // Arrange
        var leastChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 10,
        };

        var brandNewFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var mostChangedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 10,
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
                        FilePath = "files/brandNewFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/leastChangedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/mostChangedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/brandNewFile", brandNewFile.Object)
                .SeedPatch("files/leastChangedFile", leastChangedFile.Object)
                .SeedPatch("files/mostChangedFile", mostChangedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Value.Should().Be("updated most-changed-file");
    }
}
