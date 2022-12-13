using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockBranch: Mock<Branch>, IHaveSensibleDefaults<MockBranch> {
    public MockCommit? Tip { get; set; }
    public string? FriendlyName { get; set; }

    public MockBranch() {
        SetupGet(b => b.Tip).Returns(() => Tip?.Object ?? throw new NullReferenceException());
        SetupGet(b => b.FriendlyName).Returns(() => FriendlyName ?? throw new NullReferenceException());
    }

    public MockBranch WithSensibleDefaults() {
        Tip ??= new MockCommit()
            .WithSensibleDefaults();

        return this;
    }
}
