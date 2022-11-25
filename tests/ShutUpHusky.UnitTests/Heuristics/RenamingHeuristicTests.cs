using FluentAssertions;
using LibGit2Sharp;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.TestSdk.Mocks;

namespace ShutUpHusky.UnitTests.Heuristics;

public class RenamingHeuristicTests
{
    private RenamingHeuristic Heuristic => new();

    [Test]
    public void ShouldReturnRenamedLabel_ForTheFirstRenamedFile()
    {
        // Arrange
        var renamedAndChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 10,
        };

        var renamedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var anotherRenamedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
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
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedAndChangedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/anotherRenamedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/renamedAndChangedFile", renamedAndChangedFile.Object)
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch("files/anotherRenamedFile", anotherRenamedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Count.Should().Be(1);
        result.Single().Value.Should().Be("renamed renamed-file");
    }
}
