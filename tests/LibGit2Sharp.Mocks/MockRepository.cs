using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockRepository: Mock<IRepository>, IHaveSensibleDefaults<MockRepository> {
    public MockRepositoryStatus? Status { get; set; }
    public MockDiff? Diff { get; set; }
    public MockBranch? Head { get; set; }

    public MockRepository() {
        Setup(r => r.RetrieveStatus(It.IsAny<StatusOptions>())).Returns(() => Status?.Object ?? throw new NullReferenceException());
        SetupGet(r => r.Diff).Returns(() => Diff?.Object ?? throw new NullReferenceException());
        SetupGet(r => r.Head).Returns(() => Head?.Object ?? throw new NullReferenceException());
    }

    public MockRepository WithSensibleDefaults() {
        Status ??= new MockRepositoryStatus();

        Diff ??= new MockDiff();

        Head ??= new MockBranch()
            .WithSensibleDefaults();

        return this;
    }

    public MockRepository SeedPatch(string fileName, MockPatch patch, FileStatus fileStatus) {
        if (Status is null)
            throw new InvalidOperationException($"You must first set {nameof(Status)} either manually or via '{nameof(WithSensibleDefaults)}'.");
        if (Diff is null)
            throw new InvalidOperationException($"You must first set {nameof(Diff)} either manually or via '{nameof(WithSensibleDefaults)}'.");

        Status.SeedStatusEntry(
            new MockStatusEntry {
                State = fileStatus,
                FilePath = fileName,
            }
        );

        Diff.SeedPatch(fileName, patch);

        return this;
    }

    public MockRepository SeedPatch(MockStatusEntry statusEntry, MockPatch patch) {
        if (Status is null)
            throw new InvalidOperationException($"You must first set {nameof(Status)} either manually or via '{nameof(WithSensibleDefaults)}'.");
        if (Diff is null)
            throw new InvalidOperationException($"You must first set {nameof(Diff)} either manually or via '{nameof(WithSensibleDefaults)}'.");

        Status.SeedStatusEntry(statusEntry);
        Diff.SeedPatch(statusEntry.FilePath!, patch); // null forgive: MockStatusEntry will provide a decent error message if FilePath is null

        return this;
    }

    public MockRepository SeedStatusEntry(MockStatusEntry statusEntry) {
        if (Status is null)
            throw new InvalidOperationException($"You must first set {nameof(Status)} either manually or via '{nameof(WithSensibleDefaults)}'.");

        Status.SeedStatusEntry(statusEntry);

        return this;
    }
}
