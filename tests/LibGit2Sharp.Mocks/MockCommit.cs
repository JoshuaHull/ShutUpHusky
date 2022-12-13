using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockCommit: Mock<Commit>, IHaveSensibleDefaults<MockCommit> {
    public MockTree? Tree { get; set; }

    public MockCommit() {
        SetupGet(c => c.Tree).Returns(() => Tree?.Object ?? throw new NullReferenceException());
    }

    public MockCommit WithSensibleDefaults() {
        Tree ??= new MockTree()
            .WithSensibleDefaults();

        return this;
    }
}
