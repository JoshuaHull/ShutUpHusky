using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.UnitTests.Heuristics;

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
                        FilePath = "files/changedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.RenamedInIndex,
                        FilePath = "files/renamedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.DeletedFromIndex,
                        FilePath = "files/deletedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/changedFile", changedFile.Object)
                .SeedPatch("files/renamedFile", renamedFile.Object)
                .SeedPatch("files/deletedFile", deletedFile.Object)
                .Object
        };

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
                        FilePath = "files/singleCreatedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.ModifiedInIndex,
                        FilePath = "files/changedFile",
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/singleCreatedFile", singleCreatedFile.Object)
                .SeedPatch("files/changedFile", changedFile.Object)
                .Object
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HigherPriorty,
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
                        FilePath = "files/updatedFile",
                    }.Object,
                    new MockStatusEntry {
                        State = FileStatus.NewInIndex,
                        FilePath = fileName,
                    }.Object,
                },
            }.Object,
            Diff = new MockDiff()
                .SeedPatch("files/updatedFile", updatedFile.Object)
                .SeedPatch(fileName, singleCreatedFile.Object)
                .Object
        };

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
                        FilePath = "files/mediumCreatedFile",
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
                .SeedPatch("files/mediumCreatedFile", mediumCreatedFile.Object)
                .SeedPatch("files/largeCreatedFile", largeCreatedFile.Object)
                .SeedPatch("files/modifiedFile", modifiedFile.Object)
                .Object,
        };

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HigherPriorty,
                Value = "created large-created-file",
                After = ", ",
            },
            new() {
                Priority = 8,
                Value = "created medium-created-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriority,
                Value = "created small-created-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
