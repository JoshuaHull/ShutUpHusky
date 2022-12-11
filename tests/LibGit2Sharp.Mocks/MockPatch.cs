using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockPatch: Mock<Patch> {
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
    public string? Content { get; set; }

    public MockPatch() {
        SetupGet(p => p.LinesAdded).Returns(() => LinesAdded);
        SetupGet(p => p.LinesDeleted).Returns(() => LinesDeleted);
        SetupGet(p => p.Content).Returns(() => Content ?? throw new NullReferenceException());
    }
}
