using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockCommit: Mock<Commit> {
    public Tree? Tree { get; set; }

    public MockCommit() {
        SetupGet(c => c.Tree).Returns(() => Tree ?? throw new NullReferenceException());
    }
}
