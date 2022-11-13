using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests.Heuristics;

public class CreationHeuristicTests
{
    private CreationHeuristic Heuristic => new();

    [Test]
    public void ShouldReturnCreationLabel_ForTheLargestCreatedFile()
    {
        // Arrange
        var smallCreatedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 0,
        };

        var largeCreatedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
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
                        State = FileStatus.NewInIndex,
                        FilePath = "files/smallCreatedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = "files/largeCreatedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/modifiedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/smallCreatedFile", smallCreatedFile.Object)
                .SeedPatch("files/largeCreatedFile", largeCreatedFile.Object)
                .SeedPatch("files/modifiedFile", modifiedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Value.Should().Be("created large-created-file");
    }
}
