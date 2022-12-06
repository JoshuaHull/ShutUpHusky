using System;
using System.Collections.Generic;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockRepositoryStatus: Mock<RepositoryStatus> {
    public IEnumerable<StatusEntry> Staged { get; set; } = Array.Empty<StatusEntry>();
    public IEnumerable<StatusEntry> StatusEntries { get; set; } = Array.Empty<StatusEntry>();

    public MockRepositoryStatus() {
        SetupGet(e => e.Staged).Returns(() => Staged);
        Setup(s => s.GetEnumerator()).Returns(() => StatusEntries.GetEnumerator());
    }
}