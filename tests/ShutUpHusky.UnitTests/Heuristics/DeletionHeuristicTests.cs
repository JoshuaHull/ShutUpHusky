using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests.Heuristics;

public class DeletionHeuristicTests
{
    private DeletionHeuristic Heuristic => new();

    [Test]
    public void ShouldReturnDeletionLabel_ForTheLargestDeletedFile()
    {
        // Arrange
        var smallDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 50,
        };

        var largeDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 100,
        };

        var modifiedFile = new MockPatch {
            LinesAdded = 10,
            LinesDeleted = 20,
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
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/smallDeletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/largeDeletedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/modifiedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/smallDeletedFile", smallDeletedFile.Object)
                .SeedPatch("files/largeDeletedFile", largeDeletedFile.Object)
                .SeedPatch("files/modifiedFile", modifiedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Count.Should().Be(1);
        result.Single().Value.Should().Be("deleted large-deleted-file");
    }
}
