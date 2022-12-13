using Moq;

namespace LibGit2Sharp.Mocks;

public interface IHaveSensibleDefaults<T>
    where T : Mock, IHaveSensibleDefaults<T>
{
    public T WithSensibleDefaults();
}
