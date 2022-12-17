using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;
using ShutUpHusky.Heuristics.FileHeuristics;

namespace ShutUpHusky.UnitTests.Heuristics.FileHeuristics;

public class ModificationHeuristicTests
{
    private ModificationHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoFilesChanged() {
        // Arrange
        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
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
            .SeedPatch("files/createdFile", createdFile, FileStatus.NewInIndex)
            .SeedPatch("files/renamedFile", renamedFile, FileStatus.RenamedInIndex)
            .SeedPatch("files/deletedFile", deletedFile, FileStatus.DeletedFromIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().HaveCount(0);
    }

    [Test]
    public void ShouldReturnUpdatedLabel_ForModifiedFile_WithNoLinesChanged() {
        // Arrange
        var singleChangedFile = new MockPatch {
            LinesAdded = 0,
            LinesDeleted = 0,
        };

        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/createdFile", createdFile, FileStatus.NewInIndex)
            .SeedPatch("files/singleChangedFile", singleChangedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 1,
                Value = "updated single-changed-file",
            },
        });
    }

    [TestCase("singleChangedFile", ExpectedResult = "updated single-changed-file")]
    [TestCase("singleChangedFile.ts", ExpectedResult = "updated single-changed-file")]
    [TestCase("files/SingleChangedFile.cs", ExpectedResult = "updated single-changed-file")]
    [TestCase("singleChangedFile.spec.ts", ExpectedResult = "updated single-changed-file tests")]
    [TestCase("singleChangedFile.specs.ts", ExpectedResult = "updated single-changed-file tests")]
    [TestCase("singleChangedFile.test.ts", ExpectedResult = "updated single-changed-file tests")]
    [TestCase("singleChangedFile.tests.ts", ExpectedResult = "updated single-changed-file tests")]
    public string ShouldReturnUpdatedLabel_ForSingleChangedFile(string fileName) {
        // Arrange
        var singleChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 20,
        };

        var createdFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 0,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/createdFile", createdFile, FileStatus.NewInIndex)
            .SeedPatch(fileName, singleChangedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnUpdatedLabel_ForEachChangedFile_WithDescendingPriority()
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

        var mildlyChangedFile = new MockPatch {
            LinesAdded = 50,
            LinesDeleted = 50,
        };

        var mostChangedFile = new MockPatch {
            LinesAdded = 100,
            LinesDeleted = 10,
        };

        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch("files/brandNewFile", brandNewFile, FileStatus.NewInIndex)
            .SeedPatch("files/mildlyChangedFile", mildlyChangedFile, FileStatus.ModifiedInIndex)
            .SeedPatch("files/leastChangedFile", leastChangedFile, FileStatus.ModifiedInIndex)
            .SeedPatch("files/mostChangedFile", mostChangedFile, FileStatus.ModifiedInIndex);

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = 111,
                Value = "updated most-changed-file",
            },
            new() {
                Priority = 101,
                Value = "updated mildly-changed-file",
            },
            new() {
                Priority = 61,
                Value = "updated least-changed-file",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
