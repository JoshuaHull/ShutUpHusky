using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Mocks;
using NUnit.Framework;
using ShutUpHusky.Heuristics;

namespace ShutUpHusky.UnitTests.Heuristics;

public class TypeAndScopeHeuristicTests {
    private static TypeAndScopeHeuristic Heuristic => new();

    [TestCase("feat/WEB-1234", ExpectedResult = "feat(web-1234)")]
    [TestCase("fix/Web-4321", ExpectedResult = "fix(web-4321)")]
    [TestCase("chore/WeB-1111", ExpectedResult = "chore(web-1111)")]
    [TestCase("perf/weB-2222", ExpectedResult = "perf(web-2222)")]
    [TestCase("ci/wEb-3333", ExpectedResult = "ci(web-3333)")]
    [TestCase("test/wEB-4444", ExpectedResult = "test(web-4444)")]
    [TestCase("docs/WEb-2143", ExpectedResult = "docs(web-2143)")]
    public string ShouldReturnTypeWithScope_WhenBranchNameMatchesATicket(string branchName) {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = branchName,
            }
            .WithSensibleDefaults(),
        }.WithSensibleDefaults();

        // Act & Assert
        return Heuristic.Analyse(repo.Object).Single().Value;
    }

    [TestCase("feat/add-feature", ExpectedResult = "feat")]
    [TestCase("fix/squash-bugs", ExpectedResult = "fix")]
    [TestCase("chore/cleanup-code", ExpectedResult = "chore")]
    [TestCase("perf/speed-boost", ExpectedResult = "perf")]
    [TestCase("ci/deploy-quicker", ExpectedResult = "ci")]
    [TestCase("test/cover-requirements", ExpectedResult = "test")]
    [TestCase("docs/annoy-interns", ExpectedResult = "docs")]
    [TestCase("FEAT/add-feature", ExpectedResult = "feat")]
    [TestCase("FIX/squash-bugs", ExpectedResult = "fix")]
    [TestCase("CHORE/cleanup-code", ExpectedResult = "chore")]
    [TestCase("PERF/speed-boost", ExpectedResult = "perf")]
    [TestCase("CI/deploy-quicker", ExpectedResult = "ci")]
    [TestCase("TEST/cover-requirements", ExpectedResult = "test")]
    [TestCase("DOCS/annoy-interns", ExpectedResult = "docs")]
    public string ShouldReturnTypeWithoutScope_WhenBranchNameDoesNotMatchATicket(string branchName) {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = branchName,
            }
            .WithSensibleDefaults(),
        }.WithSensibleDefaults();

        // Act & Assert
        return Heuristic.Analyse(repo.Object).Single().Value;
    }

    [Test]
    public void ShouldReturnPerfType_WhenEnoughFilesHaveBeenDeleted() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "some-branch/has-no-scope-or-ticket",
            }
            .WithSensibleDefaults(),
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/createdFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
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
                State = FileStatus.DeletedFromIndex,
                FilePath = "files/firstDeletedFile",
            },
            new MockPatch {
                LinesAdded = 0,
                LinesDeleted = 100,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.DeletedFromIndex,
                FilePath = "files/secondDeletedFile",
            },
            new MockPatch {
                LinesAdded = 0,
                LinesDeleted = 100,
            }
        );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = "perf",
                After = ": ",
            },
        });
    }

    [Test]
    public void ShouldReturnCiType_WhenEnoughFilesAppearToAffectCi() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "some-branch/has-no-scope-or-ticket",
            }
            .WithSensibleDefaults()
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/createdFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
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
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/firstCiFile.yaml",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/secondCiFile.YAML",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = "ci",
                After = ": ",
            },
        });
    }

    [Test]
    public void ShouldReturnDocsType_WhenEnoughFilesAppearToAffectDocs() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "some-branch/has-no-scope-or-ticket",
            }
            .WithSensibleDefaults()
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/createdFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
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
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/firstDocsFile.md",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/secondDocsFile.MD",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = "docs",
                After = ": ",
            },
        });
    }

    [Test]
    public void ShouldReturnTestType_WhenEnoughTestHaveBeenAltered() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "some-branch/has-no-scope-or-ticket",
            }
            .WithSensibleDefaults()
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/createdFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
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
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/firstTestFile.cs",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/secondTestFile.CS",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = "test",
                After = ": ",
            },
        });
    }

    [Test]
    public void ShouldReturnChoreType_WhenBranchNameHasNoMatches_AndUnrelatedFilesHaveBeenStaged() {
        // Arrange
        var repo = new MockRepository {
            Head = new MockBranch {
                FriendlyName = "some-branch/has-no-scope-or-ticket",
            }
            .WithSensibleDefaults()
        }
        .WithSensibleDefaults()
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.NewInIndex,
                FilePath = "files/docsFile",
            },
            new MockPatch {
                LinesAdded = 100,
                LinesDeleted = 0,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.RenamedInIndex,
                FilePath = "files/yamlFile",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/testFile.cs",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        )
        .SeedPatch(
            new MockStatusEntry {
                State = FileStatus.ModifiedInIndex,
                FilePath = "files/unrelatedFile.CS",
            },
            new MockPatch {
                LinesAdded = 50,
                LinesDeleted = 50,
            }
        );

        // Act
        var result = Heuristic.Analyse(repo.Object);

        // Assert
        result.Should().BeEquivalentTo(new List<HeuristicResult> {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = "chore",
                After = ": ",
            },
        });
    }
}
