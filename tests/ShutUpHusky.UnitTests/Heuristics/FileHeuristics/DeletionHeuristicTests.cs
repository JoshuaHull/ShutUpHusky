using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Heuristics.FileHeuristics;

namespace ShutUpHusky.UnitTests.Heuristics.FileHeuristics;

public class DeletionHeuristicTests
{
    private DeletionHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoDeletedFiles() {
        // Arrange
        var changedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var renamedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/changedFile", changedFile, FileStatus.ModifiedInIndex)
            .SeedPatch("files/renamedFile", renamedFile, FileStatus.RenamedInIndex)
            .SeedPatch("files/createdFile", createdFile, FileStatus.NewInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().HaveCount(0);
    }

    [Test]
    public void ShouldReturnDeletedLabel_ForDeletedFile_WithNoLinesChanged() {
        // Arrange
        var singleDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var changedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/singleDeletedFile", singleDeletedFile, FileStatus.DeletedFromIndex)
            .SeedPatch("files/changedFile", changedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "deleted single-deleted-file",
                After = ", ",
            },
        });
    }

    [TestCase("singleDeletedFile", ExpectedResult = "deleted single-deleted-file")]
    [TestCase("singleDeletedFile.ts", ExpectedResult = "deleted single-deleted-file")]
    [TestCase("files/SingleDeletedFile.cs", ExpectedResult = "deleted single-deleted-file")]
    [TestCase("singleDeletedFile.spec.ts", ExpectedResult = "deleted single-deleted-file tests")]
    [TestCase("singleDeletedFile.specs.ts", ExpectedResult = "deleted single-deleted-file tests")]
    [TestCase("singleDeletedFile.test.ts", ExpectedResult = "deleted single-deleted-file tests")]
    [TestCase("singleDeletedFile.tests.ts", ExpectedResult = "deleted single-deleted-file tests")]
    public string ShouldReturnDeletedLabel_ForSingleDeletedFile(string fileName) {
        // Arrange
        var singleDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 20,
        };

        var updatedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/updatedFile", updatedFile, FileStatus.ModifiedInIndex)
            .SeedPatch(fileName, singleDeletedFile, FileStatus.DeletedFromIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnDeletionLabel_ForEachDeletedFile_WithDescendingPriority()
    {
        // Arrange
        var smallDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 50,
        };

        var mediumDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 75,
        };

        var largeDeletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 100,
        };

        var modifiedFile = new MockPatch {
            LinesAdded = 10,
            LinesDeleted = 20,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/smallDeletedFile", smallDeletedFile, FileStatus.DeletedFromIndex)
            .SeedPatch("files/mediumDeletedFile", mediumDeletedFile, FileStatus.DeletedFromIndex)
            .SeedPatch("files/largeDeletedFile", largeDeletedFile, FileStatus.DeletedFromIndex)
            .SeedPatch("files/modifiedFile", modifiedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "deleted large-deleted-file",
                After = ", ",
            },
            new() {
                Priority = 5.5,
                Value = "deleted medium-deleted-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriority,
                Value = "deleted small-deleted-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
