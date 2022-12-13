using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.UnitTests.Heuristics;

public class RenamingHeuristicTests
{
    private RenamingHeuristic Heuristic => new();

    [Test]
    public void ShouldNotReturnALabel_WhenThereAreNoRenamedFiles() {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.ModifiedInIndex,
                    FilePath = "files/changedFile",
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 50,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.DeletedFromIndex,
                    FilePath = "files/deletedFile",
                },
                new MockPatch {
                    LinesAdded = 0,
                    LinesDeleted = 100,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.NewInIndex,
                    FilePath = "files/createdFile",
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().HaveCount(0);
    }

    [Test]
    public void ShouldReturnRenamedLabel_ForRenamedFile_WithNoLinesChanged() {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/singleRenamedFile",
                },
                new MockPatch {
                    LinesAdded = 0,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.ModifiedInIndex,
                    FilePath = "files/changedFile",
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.MediumPriorty,
                Value = "renamed single-renamed-file",
                After = ", ",
            },
        });
    }

    [TestCase("singleRenamedFile", ExpectedResult = "renamed single-renamed-file")]
    [TestCase("singleRenamedFile.ts", ExpectedResult = "renamed single-renamed-file")]
    [TestCase("files/SingleRenamedFile.cs", ExpectedResult = "renamed single-renamed-file")]
    [TestCase("singleRenamedFile.spec.ts", ExpectedResult = "renamed single-renamed-file tests")]
    [TestCase("singleRenamedFile.specs.ts", ExpectedResult = "renamed single-renamed-file tests")]
    [TestCase("singleRenamedFile.test.ts", ExpectedResult = "renamed single-renamed-file tests")]
    [TestCase("singleRenamedFile.tests.ts", ExpectedResult = "renamed single-renamed-file tests")]
    public string ShouldReturnRenamedLabel_ForSingleRenamedFile(string fileName) {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.ModifiedInIndex,
                    FilePath = "files/updatedFile",
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 50,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = fileName,
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnRenamedLabel_ForEachRenamedFile_WithDescendingPriority()
    {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/smallRenamedFile",
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/mediumRenamedFile",
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/largeRenamedFile",
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/modifiedAndRenamedFile",
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 20,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.MediumPriorty,
                Value = "renamed large-renamed-file",
                After = ", ",
            },
            new() {
                Priority = 3,
                Value = "renamed medium-renamed-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriority,
                Value = "renamed small-renamed-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }

    [Test]
    public void ShouldReturnMovedLabel_ForMovedFile_WithNoLinesChanged() {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/movedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/movedFile",
                        NewFilePath = "files/movedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 0,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/renamedFile",
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "moved moved-file",
                After = ", ",
            },
            new() {
                Priority = Constants.MediumPriorty,
                Value = "renamed renamed-file",
                After = ", ",
            },
        });
    }

    [TestCase("oldFiles/singleMovedFile", "newFiles/singleMovedFile", ExpectedResult = "moved single-moved-file")]
    [TestCase("oldFiles/singleMovedFile.ts", "newFiles/singleMovedFile.ts", ExpectedResult = "moved single-moved-file")]
    [TestCase("oldFiles/SingleMovedFile.cs", "newFiles/SingleMovedFile.cs", ExpectedResult = "moved single-moved-file")]
    [TestCase("oldFiles/singleMovedFile.spec.ts", "newFiles/singleMovedFile.spec.ts", ExpectedResult = "moved single-moved-file tests")]
    [TestCase("oldFiles/singleMovedFile.specs.ts", "newFiles/singleMovedFile.specs.ts", ExpectedResult = "moved single-moved-file tests")]
    [TestCase("oldFiles/singleMovedFile.test.ts", "newFiles/singleMovedFile.test.ts", ExpectedResult = "moved single-moved-file tests")]
    [TestCase("oldFiles/singleMovedFile.tests.ts", "newFiles/singleMovedFile.tests.ts", ExpectedResult = "moved single-moved-file tests")]
    public string ShouldReturnMovedLabel_ForSingleMovedFile(string fromFileName, string toFileName) {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/renamedFile",
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 50,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = toFileName,
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = fromFileName,
                        NewFilePath = toFileName,
                    },
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        return result.Single().Value;
    }

    [Test]
    public void ShouldReturnMovedLabel_ForEachMovedFile_WithDescendingPriority()
    {
        // Arrange
        var repo = new MockRepository()
            .WithSensibleDefaults()
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/simplyRenamedFile",
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/mediumMovedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/mediumMovedFile",
                        NewFilePath = "files/mediumMovedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 50,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/largeMovedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/largeMovedFile",
                        NewFilePath = "files/largeMovedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 100,
                    LinesDeleted = 0,
                }
            )
            .SeedPatch(
                new MockStatusEntry {
                    State = FileStatus.RenamedInIndex,
                    FilePath = "files/modifiedAndMovedFile",
                    HeadToIndexRenameDetails = new MockRenameDetails {
                        OldFilePath = "oldFiles/modifiedAndMovedFile",
                        NewFilePath = "files/modifiedAndMovedFile",
                    },
                },
                new MockPatch {
                    LinesAdded = 10,
                    LinesDeleted = 20,
                }
            );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.HighPriorty,
                Value = "moved large-moved-file",
                After = ", ",
            },
            new() {
                Priority = 5.5,
                Value = "moved medium-moved-file",
                After = ", ",
            },
            new() {
                Priority = Constants.LowPriority,
                Value = "moved modified-and-moved-file",
                After = ", ",
            },
            new() {
                Priority = Constants.MediumPriorty,
                Value = "renamed simply-renamed-file",
                After = ", ",
            },
        }).And.BeInDescendingOrder(h => h.Priority);
    }
}
