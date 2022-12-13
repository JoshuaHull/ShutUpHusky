using System.Collections.Generic;
using System.Linq;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockRepositoryStatus: Mock<RepositoryStatus> {
    private List<MockStatusEntry> _staged = new();
    public IEnumerable<MockStatusEntry> Staged {
        get => _staged;
        set => _staged = value.ToList();
    }

    private List<MockStatusEntry> _statusEntries = new();
    public IEnumerable<MockStatusEntry> StatusEntries {
        get => _statusEntries;
        set => _statusEntries = value.ToList();
    }

    public MockRepositoryStatus() {
        SetupGet(e => e.Staged).Returns(() => Staged.Select(s => s.Object));
        Setup(s => s.GetEnumerator()).Returns(() => StatusEntries.Select(e => e.Object).GetEnumerator());
    }

    public MockRepositoryStatus SeedStatusEntry(MockStatusEntry entry) {
        _statusEntries.Add(entry);
        return this;
    }
}
