using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockBranch: Mock<Branch>, IHaveSensibleDefaults<MockBranch> {
    public Commit? Tip { get; set; }
    public string? FriendlyName { get; set; }

    public MockBranch() {
        SetupGet(b => b.Tip).Returns(() => Tip ?? throw new NullReferenceException());
        SetupGet(b => b.FriendlyName).Returns(() => FriendlyName ?? throw new NullReferenceException());
    }

    public MockBranch WithSensibleDefaults() {
        Tip ??= new MockCommit {
            Tree = new MockTree {
            }.Object,
        }.Object;

        return this;
    }
}
