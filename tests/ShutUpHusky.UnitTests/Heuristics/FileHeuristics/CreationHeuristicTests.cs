using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Heuristics.FileHeuristics;

namespace ShutUpHusky.UnitTests.Heuristics.FileHeuristics;

public class CreationHeuristicTests
{
    private CreationHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoNewFiles() {
        // Arrange
        var changedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var renamedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var deletedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 100,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("src/changedFile", changedFile, FileStatus.ModifiedInIndex)
            .SeedPatch("src/renamedFile", renamedFile, FileStatus.RenamedInIndex)
            .SeedPatch("src/deletedFile", deletedFile, FileStatus.DeletedFromIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().HaveCount(0);
    }

    [Test]
    public void ShouldReturnCreatedLabel_ForNewFile_WithNoLinesChanged() {
        // Arrange
        var singleCreatedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var changedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/singleCreatedFile", singleCreatedFile, FileStatus.NewInIndex)
            .SeedPatch("files/changedFile", changedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 1,
                Value = "created single-created-file",
                After = ", ",
            },
        });
    }

    [TestCase("singleCreatedFile", ExpectedResult = "created single-created-file")]
    [TestCase("singleCreatedFile.ts", ExpectedResult = "created single-created-file")]
    [TestCase("files/SingleCreatedFile.cs", ExpectedResult = "created single-created-file")]
    [TestCase("singleCreatedFile.spec.ts", ExpectedResult = "created single-created-file tests")]
    [TestCase("singleCreatedFile.specs.ts", ExpectedResult = "created single-created-file tests")]
    [TestCase("singleCreatedFile.test.ts", ExpectedResult = "created single-created-file tests")]
    [TestCase("singleCreatedFile.tests.ts", ExpectedResult = "created single-created-file tests")]
    public string ShouldReturnCreatedLabel_ForSingleCreatedFile(string fileName) {
        // Arrange
        var singleCreatedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 20,
        };

        var updatedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/updatedFile", updatedFile, FileStatus.ModifiedInIndex)
            .SeedPatch(fileName, singleCreatedFile, FileStatus.NewInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnCreationLabel_ForEachChangedFile_WithDescendingPriority()
    {
        // Arrange
        var smallCreatedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 0,
        };

        var mediumCreatedFile = new MockPatch {
            LinesAdded = 75,
            LinesDeleted = 0,
        };

        var largeCreatedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var modifiedFile = new MockPatch {
            LinesAdded = 120,
            LinesDeleted = 0,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/smallCreatedFile", smallCreatedFile, FileStatus.NewInIndex)
            .SeedPatch("files/mediumCreatedFile", mediumCreatedFile, FileStatus.NewInIndex)
            .SeedPatch("files/largeCreatedFile", largeCreatedFile, FileStatus.NewInIndex)
            .SeedPatch("files/modifiedFile", modifiedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 101,
                Value = "created large-created-file",
                After = ", ",
            },
            new() {
                Priority = 76,
                Value = "created medium-created-file",
                After = ", ",
            },
            new() {
                Priority = 51,
                Value = "created small-created-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
