using Moq;

namespace LibGit2Sharp.Mocks;

public interface IHaveSensibleDefaults {
}

public interface IHaveSensibleDefaults<T> : IHaveSensibleDefaults
    where T : Mock, IHaveSensibleDefaults<T>
{
    public T WithSensibleDefaults();
}
