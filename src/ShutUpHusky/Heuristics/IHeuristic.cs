using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

public interface IHeuristic {
    HeuristicResult Analyse(IRepository repo);
}
