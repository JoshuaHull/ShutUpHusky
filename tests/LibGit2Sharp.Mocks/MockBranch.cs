using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockBranch: Mock<Branch> {
    public Commit? Tip { get; set; }
    public string? FriendlyName { get; set; }

    public MockBranch() {
        SetupGet(b => b.Tip).Returns(() => Tip ?? throw new NullReferenceException());
        SetupGet(b => b.FriendlyName).Returns(() => FriendlyName ?? throw new NullReferenceException());
    }
}
