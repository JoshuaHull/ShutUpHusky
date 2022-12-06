using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockRenameDetails : Mock<RenameDetails> {
    public string? NewFilePath { get; set; }
    public string? OldFilePath { get; set; }

    public MockRenameDetails() {
        SetupGet(e => e.NewFilePath).Returns(() => NewFilePath ?? throw new NullReferenceException());
        SetupGet(e => e.OldFilePath).Returns(() => OldFilePath ?? throw new NullReferenceException());
    }
}
