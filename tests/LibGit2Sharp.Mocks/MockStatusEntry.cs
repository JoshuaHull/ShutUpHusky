using System;
using Moq;

namespace LibGit2Sharp.Mocks;

public class MockStatusEntry : Mock<StatusEntry> {
    public string? FilePath { get; set; }
    public FileStatus? State { get; set; }
    public MockRenameDetails? HeadToIndexRenameDetails { get; set; }

    public MockStatusEntry() {
        SetupGet(e => e.FilePath).Returns(() => FilePath ?? throw new NullReferenceException());
        SetupGet(e => e.State).Returns(() => State ?? throw new NullReferenceException());
        SetupGet(e => e.HeadToIndexRenameDetails).Returns(() => HeadToIndexRenameDetails?.Object ?? throw new NullReferenceException());
    }
}
